using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Transform Recorder 전체 관리자
/// - Recorder 목록 관리
/// - 전체 Start / Stop / Clear 제어
/// </summary>
public class scr_TransformRecorderManager : MonoBehaviour
{
    [Header("Tracked Recorders")]
    [SerializeField] private List<scr_TransformRecorder> recorders = new List<scr_TransformRecorder>();

    [Header("Auto Settings")]
    [SerializeField] private bool autoFindRecordersOnStart = true;
    [SerializeField] private bool autoStartRecordingOnPlay = true;

    private void Start()
    {
        if (autoFindRecordersOnStart)
        {
            FindAllRecordersInScene();
        }

        if (autoStartRecordingOnPlay)
        {
            StartAllRecordings();
        }
    }

    [ContextMenu("Find All Recorders In Scene")]
    public void FindAllRecordersInScene()
    {
        scr_TransformRecorder[] foundRecorders =
            FindObjectsByType<scr_TransformRecorder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        recorders.Clear();
        recorders.AddRange(foundRecorders);
    }

    [ContextMenu("Start All Recordings")]
    public void StartAllRecordings()
    {
        ForEachRecorder(recorder => recorder.StartRecording());
    }

    [ContextMenu("Stop All Recordings")]
    public void StopAllRecordings()
    {
        ForEachRecorder(recorder => recorder.StopRecording());
    }

    [ContextMenu("Clear And Restart All Recordings")]
    public void ClearAndRestartAllRecordings()
    {
        ForEachRecorder(recorder => recorder.ClearAndRestartRecording());
    }

    public List<scr_TransformRecorder> GetRecorders()
    {
        return new List<scr_TransformRecorder>(recorders);
    }

    private void ForEachRecorder(System.Action<scr_TransformRecorder> action)
    {
        for (int i = 0; i < recorders.Count; i++)
        {
            scr_TransformRecorder recorder = recorders[i];
            if (recorder == null)
            {
                continue;
            }

            action?.Invoke(recorder);
        }
    }
}
