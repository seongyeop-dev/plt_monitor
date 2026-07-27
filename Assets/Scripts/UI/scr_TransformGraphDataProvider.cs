using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Graph UI에 전달할 Transform 데이터 제공
/// - Recorder 수집
/// - Object 이름 목록 제공
/// - 선택한 Object / Category / Axis 기준 그래프 데이터 반환
/// - 필요 시 Reflection fallback 지원
/// </summary>
public class scr_TransformGraphDataProvider : MonoBehaviour
{
    public enum TransformCategory
    {
        Position,
        Rotation,
        Scale
    }

    public enum AxisType
    {
        X,
        Y,
        Z
    }

    [Serializable]
    public class GraphPoint
    {
        public int frameIndex;
        public float timeStamp;
        public float value;

        public GraphPoint(int frameIndex, float timeStamp, float value)
        {
            this.frameIndex = frameIndex;
            this.timeStamp = timeStamp;
            this.value = value;
        }
    }

    [Header("Recorder Source")]
    [SerializeField] private Transform recorderSearchRoot;
    [SerializeField] private bool autoCollectOnAwake = true;
    [SerializeField] private bool includeInactive = true;

    [Header("Recorder Cache")]
    [SerializeField] private List<scr_TransformRecorder> recorderList = new List<scr_TransformRecorder>();

    private readonly Dictionary<string, scr_TransformRecorder> recorderMap = new Dictionary<string, scr_TransformRecorder>();
    private readonly List<string> cachedObjectNames = new List<string>();

    public IReadOnlyList<scr_TransformRecorder> RecorderList => recorderList;

    private void Awake()
    {
        if (autoCollectOnAwake)
        {
            CollectRecorders();
        }
        else
        {
            RebuildRecorderMap();
        }
    }

    [ContextMenu("Collect Recorders")]
    public void CollectRecorders()
    {
        recorderList.Clear();

        if (recorderSearchRoot != null)
        {
            scr_TransformRecorder[] recorders = recorderSearchRoot.GetComponentsInChildren<scr_TransformRecorder>(includeInactive);
            recorderList.AddRange(recorders);
        }
        else
        {
            scr_TransformRecorder[] recorders = FindObjectsByType<scr_TransformRecorder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            recorderList.AddRange(recorders);
        }

        RebuildRecorderMap();
    }

    public void SetRecorders(List<scr_TransformRecorder> newRecorderList)
    {
        recorderList = newRecorderList ?? new List<scr_TransformRecorder>();
        RebuildRecorderMap();
    }

    private void RebuildRecorderMap()
    {
        recorderMap.Clear();
        cachedObjectNames.Clear();

        for (int i = 0; i < recorderList.Count; i++)
        {
            scr_TransformRecorder recorder = recorderList[i];

            if (recorder == null)
            {
                continue;
            }

            string objectName = recorder.gameObject.name;

            if (recorderMap.ContainsKey(objectName))
            {
                Debug.LogWarning($"[GraphDataProvider] Duplicate recorder object name detected: {objectName}");
                continue;
            }

            recorderMap.Add(objectName, recorder);
            cachedObjectNames.Add(objectName);
        }

        cachedObjectNames.Sort();
    }

    public List<string> GetObjectNameList()
    {
        return new List<string>(cachedObjectNames);
    }

    public bool HasRecorder(string objectName)
    {
        return !string.IsNullOrWhiteSpace(objectName) && recorderMap.ContainsKey(objectName);
    }

    public bool TryGetRecorder(string objectName, out scr_TransformRecorder recorder)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            recorder = null;
            return false;
        }

        return recorderMap.TryGetValue(objectName, out recorder);
    }

    public List<GraphPoint> GetGraphPoints(string objectName, TransformCategory category, AxisType axis)
    {
        List<GraphPoint> result = new List<GraphPoint>();

        if (!TryGetRecorder(objectName, out scr_TransformRecorder recorder))
        {
            Debug.LogWarning($"[GraphDataProvider] Recorder not found for object: {objectName}");
            return result;
        }

        List<TransformFrameData> typedFrames = recorder.GetRecordedFrames();
        if (typedFrames != null && typedFrames.Count > 0)
        {
            for (int i = 0; i < typedFrames.Count; i++)
            {
                TransformFrameData frame = typedFrames[i];
                if (frame == null)
                {
                    continue;
                }

                result.Add(new GraphPoint(
                    frame.frameIndex,
                    frame.timeStamp,
                    ExtractTypedFrameValue(frame, category, axis)));
            }

            return result;
        }

        IList rawFrameList = TryGetRawFrameList(recorder);
        if (rawFrameList == null)
        {
            Debug.LogWarning($"[GraphDataProvider] Failed to read frame list from recorder: {objectName}");
            return result;
        }

        for (int i = 0; i < rawFrameList.Count; i++)
        {
            object rawFrame = rawFrameList[i];
            if (rawFrame == null)
            {
                continue;
            }

            int frameIndex = GetIntMemberValue(rawFrame, "frameIndex", "FrameIndex", "index", "Index");
            float timeStamp = GetFloatMemberValue(rawFrame, "timeStamp", "TimeStamp", "timestamp", "Timestamp", "time", "Time");
            float value = ExtractAxisValue(rawFrame, category, axis);

            result.Add(new GraphPoint(frameIndex, timeStamp, value));
        }

        return result;
    }

    public List<GraphPoint> GetGraphPoints(int objectIndex, TransformCategory category, AxisType axis)
    {
        if (objectIndex < 0 || objectIndex >= cachedObjectNames.Count)
        {
            Debug.LogWarning($"[GraphDataProvider] Invalid object index: {objectIndex}");
            return new List<GraphPoint>();
        }

        return GetGraphPoints(cachedObjectNames[objectIndex], category, axis);
    }

    public int GetFrameCount(string objectName)
    {
        if (!TryGetRecorder(objectName, out scr_TransformRecorder recorder))
        {
            return 0;
        }

        return recorder.CurrentFrameCount;
    }

    public bool TryGetMinMax(string objectName, TransformCategory category, AxisType axis, out float minValue, out float maxValue)
    {
        minValue = 0f;
        maxValue = 0f;

        List<GraphPoint> points = GetGraphPoints(objectName, category, axis);
        if (points == null || points.Count == 0)
        {
            return false;
        }

        minValue = points[0].value;
        maxValue = points[0].value;

        for (int i = 1; i < points.Count; i++)
        {
            float value = points[i].value;
            if (value < minValue) minValue = value;
            if (value > maxValue) maxValue = value;
        }

        return true;
    }

    private float ExtractTypedFrameValue(TransformFrameData frame, TransformCategory category, AxisType axis)
    {
        switch (category)
        {
            case TransformCategory.Position:
                return axis switch
                {
                    AxisType.X => frame.posX,
                    AxisType.Y => frame.posY,
                    AxisType.Z => frame.posZ,
                    _ => 0f
                };

            case TransformCategory.Rotation:
                return axis switch
                {
                    AxisType.X => frame.rotX,
                    AxisType.Y => frame.rotY,
                    AxisType.Z => frame.rotZ,
                    _ => 0f
                };

            case TransformCategory.Scale:
                return axis switch
                {
                    AxisType.X => frame.scaleX,
                    AxisType.Y => frame.scaleY,
                    AxisType.Z => frame.scaleZ,
                    _ => 0f
                };

            default:
                return 0f;
        }
    }

    private IList TryGetRawFrameList(scr_TransformRecorder recorder)
    {
        if (recorder == null)
        {
            return null;
        }

        Type recorderType = recorder.GetType();

        string[] methodNames =
        {
            "GetRecordedFrames",
            "GetFrameDataList",
            "GetFrames",
            "GetRecords",
            "GetRecordedData"
        };

        for (int i = 0; i < methodNames.Length; i++)
        {
            MethodInfo method = recorderType.GetMethod(methodNames[i], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null && method.GetParameters().Length == 0)
            {
                object methodResult = method.Invoke(recorder, null);
                if (methodResult is IList methodList)
                {
                    return methodList;
                }
            }
        }

        string[] propertyNames =
        {
            "RecordedFrames",
            "FrameDataList",
            "Frames",
            "Records",
            "RecordedData"
        };

        for (int i = 0; i < propertyNames.Length; i++)
        {
            PropertyInfo property = recorderType.GetProperty(propertyNames[i], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null)
            {
                object propertyResult = property.GetValue(recorder);
                if (propertyResult is IList propertyList)
                {
                    return propertyList;
                }
            }
        }

        string[] fieldNames =
        {
            "recordedFrames",
            "frameDataList",
            "frames",
            "records",
            "recordedData"
        };

        for (int i = 0; i < fieldNames.Length; i++)
        {
            FieldInfo field = recorderType.GetField(fieldNames[i], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                object fieldResult = field.GetValue(recorder);
                if (fieldResult is IList fieldList)
                {
                    return fieldList;
                }
            }
        }

        return null;
    }

    private float ExtractAxisValue(object rawFrame, TransformCategory category, AxisType axis)
    {
        switch (category)
        {
            case TransformCategory.Position:
                return ExtractPositionValue(rawFrame, axis);
            case TransformCategory.Rotation:
                return ExtractRotationValue(rawFrame, axis);
            case TransformCategory.Scale:
                return ExtractScaleValue(rawFrame, axis);
            default:
                return 0f;
        }
    }

    private float ExtractPositionValue(object rawFrame, AxisType axis)
    {
        return axis switch
        {
            AxisType.X => GetFloatMemberValue(rawFrame, "posX", "PosX", "positionX", "PositionX"),
            AxisType.Y => GetFloatMemberValue(rawFrame, "posY", "PosY", "positionY", "PositionY"),
            AxisType.Z => GetFloatMemberValue(rawFrame, "posZ", "PosZ", "positionZ", "PositionZ"),
            _ => 0f
        };
    }

    private float ExtractRotationValue(object rawFrame, AxisType axis)
    {
        return axis switch
        {
            AxisType.X => GetFloatMemberValue(rawFrame, "rotX", "RotX", "rotationX", "RotationX"),
            AxisType.Y => GetFloatMemberValue(rawFrame, "rotY", "RotY", "rotationY", "RotationY"),
            AxisType.Z => GetFloatMemberValue(rawFrame, "rotZ", "RotZ", "rotationZ", "RotationZ"),
            _ => 0f
        };
    }

    private float ExtractScaleValue(object rawFrame, AxisType axis)
    {
        return axis switch
        {
            AxisType.X => GetFloatMemberValue(rawFrame, "scaleX", "ScaleX"),
            AxisType.Y => GetFloatMemberValue(rawFrame, "scaleY", "ScaleY"),
            AxisType.Z => GetFloatMemberValue(rawFrame, "scaleZ", "ScaleZ"),
            _ => 0f
        };
    }

    private int GetIntMemberValue(object target, params string[] memberNames)
    {
        return Mathf.RoundToInt(GetFloatMemberValue(target, memberNames));
    }

    private float GetFloatMemberValue(object target, params string[] memberNames)
    {
        if (target == null)
        {
            return 0f;
        }

        Type targetType = target.GetType();

        for (int i = 0; i < memberNames.Length; i++)
        {
            string memberName = memberNames[i];

            FieldInfo field = targetType.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null && TryConvertToFloat(field.GetValue(target), out float fieldValue))
            {
                return fieldValue;
            }

            PropertyInfo property = targetType.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null && TryConvertToFloat(property.GetValue(target), out float propertyValue))
            {
                return propertyValue;
            }
        }

        return 0f;
    }

    private bool TryConvertToFloat(object value, out float result)
    {
        result = 0f;

        if (value == null)
        {
            return false;
        }

        try
        {
            result = Convert.ToSingle(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    [ContextMenu("Debug Print Object Names")]
    public void DebugPrintObjectNames()
    {
        Debug.Log($"[GraphDataProvider] Recorder Count = {cachedObjectNames.Count}");

        for (int i = 0; i < cachedObjectNames.Count; i++)
        {
            Debug.Log($"[GraphDataProvider] Object[{i}] = {cachedObjectNames[i]}");
        }
    }
}
