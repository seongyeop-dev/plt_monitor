using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Yeop;

/// <summary>
/// Transform Monitoring 单捞磐 历厘 包府磊
/// </summary>
public class scr_TransformDataSaveLoadManager : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private scr_TransformRecorderManager recorderManager;

    [Header("Save Settings")]
    [SerializeField] private string rootFolderName = "PLT_Monitor";
    [SerializeField] private string saveFolderName = "TransformRecords";
    [SerializeField] private string jsonFileName = "transform_record.json";

    public string GetSaveFolderPath()
    {
        return Path.Combine(Application.persistentDataPath, rootFolderName, saveFolderName);
    }

    public string GetDefaultJsonPath()
    {
        return Path.Combine(GetSaveFolderPath(), jsonFileName);
    }

    public string GetNewCsvPath()
    {
        string fileName = "transform_record_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        return Path.Combine(GetSaveFolderPath(), fileName);
    }

    public TransformMonitoringSaveData BuildSaveData()
    {
        TransformMonitoringSaveData saveData = new TransformMonitoringSaveData
        {
            sessionName = "PLT_Monitor_Session",
            createdTimeText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        if (recorderManager == null)
        {
            Debug.LogWarning("[scr_TransformDataSaveLoadManager] RecorderManager is not assigned.");
            return saveData;
        }

        List<scr_TransformRecorder> recorders = recorderManager.GetRecorders();

        for (int i = 0; i < recorders.Count; i++)
        {
            scr_TransformRecorder recorder = recorders[i];
            if (recorder == null)
            {
                continue;
            }

            TransformObjectRecordData objectData = new TransformObjectRecordData
            {
                objectName = recorder.TargetName,
                frames = recorder.GetRecordedFrames()
            };

            saveData.objects.Add(objectData);
        }

        return saveData;
    }

    [ContextMenu("Save JSON")]
    public void SaveJson()
    {
        TransformMonitoringSaveData saveData = BuildSaveData();
        string jsonPath = GetDefaultJsonPath();
        NST_Json.ExportJson(saveData, jsonPath);
        Debug.Log("[scr_TransformDataSaveLoadManager] JSON Saved : " + jsonPath);
    }

    [ContextMenu("Save CSV")]
    public void SaveCsv()
    {
        TransformMonitoringSaveData saveData = BuildSaveData();
        List<Dictionary<string, object>> csvRows = ConvertSaveDataToCsvRows(saveData);

        if (csvRows.Count == 0)
        {
            Debug.LogWarning("[scr_TransformDataSaveLoadManager] No frame data to export.");
            return;
        }

        string csvPath = GetNewCsvPath();
        NST_CSV.ExportCSV(csvRows, csvPath);
        Debug.Log("[scr_TransformDataSaveLoadManager] CSV Saved : " + csvPath);
    }

    public TransformMonitoringSaveData LoadJsonFromPath(string jsonPath)
    {
        if (string.IsNullOrWhiteSpace(jsonPath) || !File.Exists(jsonPath))
        {
            Debug.LogWarning("[scr_TransformDataSaveLoadManager] JSON file does not exist : " + jsonPath);
            return null;
        }

        TransformMonitoringSaveData saveData = NST_Json.ImportJson<TransformMonitoringSaveData>(jsonPath);
        if (saveData == null)
        {
            Debug.LogWarning("[scr_TransformDataSaveLoadManager] Failed to load JSON : " + jsonPath);
            return null;
        }

        Debug.Log("[scr_TransformDataSaveLoadManager] JSON Loaded : " + jsonPath);
        return saveData;
    }

    [ContextMenu("Load Default JSON")]
    public TransformMonitoringSaveData LoadDefaultJson()
    {
        return LoadJsonFromPath(GetDefaultJsonPath());
    }

    public List<Dictionary<string, object>> LoadCsvFromPath(string csvPath)
    {
        if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
        {
            Debug.LogWarning("[scr_TransformDataSaveLoadManager] CSV file does not exist : " + csvPath);
            return null;
        }

        List<Dictionary<string, object>> rows = NST_CSV.ImportCsvFromFile(csvPath);
        if (rows == null)
        {
            Debug.LogWarning("[scr_TransformDataSaveLoadManager] Failed to load CSV : " + csvPath);
            return null;
        }

        Debug.Log("[scr_TransformDataSaveLoadManager] CSV Loaded : " + csvPath);
        return rows;
    }

    public List<string> GetSavedFiles(string searchPattern)
    {
        List<string> filePaths = new List<string>();
        string folderPath = GetSaveFolderPath();

        if (!Directory.Exists(folderPath))
        {
            return filePaths;
        }

        string[] files = Directory.GetFiles(folderPath, searchPattern, SearchOption.TopDirectoryOnly);
        filePaths.AddRange(files);
        filePaths.Sort();
        filePaths.Reverse();
        return filePaths;
    }

    private List<Dictionary<string, object>> ConvertSaveDataToCsvRows(TransformMonitoringSaveData saveData)
    {
        List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

        if (saveData == null || saveData.objects == null)
        {
            return rows;
        }

        for (int i = 0; i < saveData.objects.Count; i++)
        {
            TransformObjectRecordData objectData = saveData.objects[i];
            if (objectData == null || objectData.frames == null)
            {
                continue;
            }

            for (int j = 0; j < objectData.frames.Count; j++)
            {
                TransformFrameData frame = objectData.frames[j];
                if (frame == null)
                {
                    continue;
                }

                rows.Add(BuildCsvRow(saveData, objectData.objectName, frame));
            }
        }

        return rows;
    }

    private Dictionary<string, object> BuildCsvRow(
        TransformMonitoringSaveData saveData,
        string objectName,
        TransformFrameData frame)
    {
        Dictionary<string, object> row = new Dictionary<string, object>
        {
            ["SessionName"] = saveData.sessionName,
            ["CreatedTime"] = saveData.createdTimeText,
            ["ObjectName"] = objectName,
            ["FrameIndex"] = frame.frameIndex,
            ["TimeStamp"] = frame.timeStamp,
            ["PosX"] = frame.posX,
            ["PosY"] = frame.posY,
            ["PosZ"] = frame.posZ,
            ["RotX"] = frame.rotX,
            ["RotY"] = frame.rotY,
            ["RotZ"] = frame.rotZ,
            ["ScaleX"] = frame.scaleX,
            ["ScaleY"] = frame.scaleY,
            ["ScaleZ"] = frame.scaleZ
        };

        return row;
    }
}
