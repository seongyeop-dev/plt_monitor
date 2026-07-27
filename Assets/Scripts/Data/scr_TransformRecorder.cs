using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 단일 오브젝트 Transform 기록기
/// - Position / Rotation / Scale 기록
/// - 최대 프레임 수 버퍼 유지
/// </summary>
public class scr_TransformRecorder : MonoBehaviour
{
    [Header("Record Settings")]
    [SerializeField] private bool recordOnStart = true;
    [SerializeField] private int maxFrameCount = 600;
    [SerializeField] private bool recordWhenInactive = false;

    private readonly List<TransformFrameData> recordedFrames = new List<TransformFrameData>();

    private bool isRecording;
    private int recordedFrameIndex;

    public string TargetName => gameObject.name;
    public bool IsRecording => isRecording;
    public int MaxFrameCount => maxFrameCount;
    public int CurrentFrameCount => recordedFrames.Count;

    private void Start()
    {
        if (recordOnStart)
        {
            StartRecording();
        }
    }

    private void Update()
    {
        if (!CanRecordThisFrame())
        {
            return;
        }

        RecordCurrentFrame();
    }

    public void StartRecording()
    {
        isRecording = true;
    }

    public void StopRecording()
    {
        isRecording = false;
    }

    public void ClearAndRestartRecording()
    {
        ClearRecordedFrames();
        recordedFrameIndex = 0;
        isRecording = true;
    }

    public void ClearRecordedFrames()
    {
        recordedFrames.Clear();
    }

    public List<TransformFrameData> GetRecordedFrames()
    {
        return new List<TransformFrameData>(recordedFrames);
    }

    public TransformFrameData GetLatestFrame()
    {
        if (recordedFrames.Count == 0)
        {
            return null;
        }

        return recordedFrames[recordedFrames.Count - 1];
    }

    private bool CanRecordThisFrame()
    {
        if (!isRecording)
        {
            return false;
        }

        if (!recordWhenInactive && !gameObject.activeInHierarchy)
        {
            return false;
        }

        return true;
    }

    private void RecordCurrentFrame()
    {
        recordedFrames.Add(new TransformFrameData(recordedFrameIndex, Time.time, transform));
        recordedFrameIndex++;
        TrimFrameBufferIfNeeded();
    }

    private void TrimFrameBufferIfNeeded()
    {
        int overflowCount = recordedFrames.Count - maxFrameCount;
        if (overflowCount <= 0)
        {
            return;
        }

        recordedFrames.RemoveRange(0, overflowCount);
    }
}
