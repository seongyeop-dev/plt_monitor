using UnityEngine;

/// <summary>
/// 오브젝트 자동 회전
/// </summary>
public class scr_AutoRotateObject : MonoBehaviour
{
    [Header("Rotate Settings")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float rotationSpeed = 60f;

    private void Update()
    {
        if (rotationAxis == Vector3.zero || Mathf.Approximately(rotationSpeed, 0f))
        {
            return;
        }

        transform.Rotate(rotationAxis.normalized * rotationSpeed * Time.deltaTime);
    }
}
