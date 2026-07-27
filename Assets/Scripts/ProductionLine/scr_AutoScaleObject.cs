using UnityEngine;

/// <summary>
/// 오브젝트 자동 스케일 변화
/// </summary>
public class scr_AutoScaleObject : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private Vector3 baseScale = Vector3.one;
    [SerializeField] private Vector3 scaleAmplitude = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] private float scaleSpeed = 2.0f;

    private void Update()
    {
        float t = (Mathf.Sin(Time.time * scaleSpeed) + 1f) * 0.5f;
        Vector3 offset = new Vector3(
            scaleAmplitude.x * t,
            scaleAmplitude.y * t,
            scaleAmplitude.z * t);

        transform.localScale = baseScale + offset;
    }
}
