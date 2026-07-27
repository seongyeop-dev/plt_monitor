using System;
using System.Collections.Generic;

/// <summary>
/// 전체 Transform Monitoring 세션 저장 데이터
/// </summary>
[Serializable]
public class TransformMonitoringSaveData
{
    public string sessionName;
    public string createdTimeText;
    public List<TransformObjectRecordData> objects = new List<TransformObjectRecordData>();
}