using System;
using System.Collections.Generic;

/// <summary>
/// 오브젝트 1개의 전체 Transform 기록 데이터
/// </summary>
[Serializable]
public class TransformObjectRecordData
{
    public string objectName;
    public List<TransformFrameData> frames = new List<TransformFrameData>();
}