using UnityEngine;

/// <summary>
/// 컨베이어 위 개별 박스 이동 
/// - 이동만 담당
/// - Spawn / Recycle / 상태 관리는 Manager 담당
/// </summary>
public class scr_ConveyorItemMover : MonoBehaviour
{
    private Transform startPoint;
    private Transform endPoint;

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private bool useWorldAxisMove = false;
    [SerializeField] private Vector3 worldMoveDirection = Vector3.forward;

    [Header("End Check")]
    [SerializeField] private float endDistanceThreshold = 0.1f;

    private Vector3 moveDirection = Vector3.zero;
    private bool isMoving;

    public void Initialize(Transform assignedStartPoint, Transform assignedEndPoint, float assignedSpeed)
    {
        startPoint = assignedStartPoint;
        endPoint = assignedEndPoint;
        moveSpeed = assignedSpeed;
        InitializeMoveDirection();
    }

    public void ActivateAtStartPoint()
    {
        if (startPoint == null)
        {
            Debug.LogWarning($"[{nameof(scr_ConveyorItemMover)}] StartPoint is not assigned on {gameObject.name}");
            return;
        }

        transform.position = startPoint.position;
        gameObject.SetActive(true);
        isMoving = true;
    }

    public void DeactivateItem()
    {
        isMoving = false;
        gameObject.SetActive(false);
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public bool HasReachedEndPoint()
    {
        if (endPoint == null)
        {
            return false;
        }

        return Vector3.Distance(transform.position, endPoint.position) <= endDistanceThreshold;
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    private void Update()
    {
        if (!CanMove())
        {
            return;
        }

        MoveItem();
    }

    private bool CanMove()
    {
        return isMoving && startPoint != null && endPoint != null && moveDirection != Vector3.zero;
    }

    private void InitializeMoveDirection()
    {
        if (startPoint == null || endPoint == null)
        {
            moveDirection = Vector3.zero;
            return;
        }

        if (useWorldAxisMove)
        {
            moveDirection = worldMoveDirection.normalized;
            return;
        }

        moveDirection = (endPoint.position - startPoint.position).normalized;
    }

    private void MoveItem()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}
