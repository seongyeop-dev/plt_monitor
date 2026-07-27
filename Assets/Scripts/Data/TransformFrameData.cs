using System;
using UnityEngine;

/// <summary>
/// 오브젝트 1개의 한 프레임 Transform 데이터
/// </summary>
[Serializable]
public class TransformFrameData
{
    public int frameIndex;
    public float timeStamp;

    public float posX;
    public float posY;
    public float posZ;

    public float rotX;
    public float rotY;
    public float rotZ;

    public float scaleX;
    public float scaleY;
    public float scaleZ;

    // JSON 역직렬화용 기본 생성자
    public TransformFrameData()
    {
    }

    public TransformFrameData(int frameIndex, float timeStamp, Transform target)
    {
        this.frameIndex = frameIndex;
        this.timeStamp = timeStamp;

        if (target == null)
        {
            return;
        }

        Vector3 position = target.position;
        Vector3 rotation = target.eulerAngles;
        Vector3 scale = target.localScale;

        posX = position.x;
        posY = position.y;
        posZ = position.z;

        rotX = rotation.x;
        rotY = rotation.y;
        rotZ = rotation.z;

        scaleX = scale.x;
        scaleY = scale.y;
        scaleZ = scale.z;
    }
}