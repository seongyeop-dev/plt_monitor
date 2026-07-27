using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 컨베이어 Spawn + Pool 관리자
/// </summary>
public class scr_ConveyorSpawnManager : MonoBehaviour
{
    [Header("Line Points")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [Header("Box Pool")]
    [SerializeField] private List<scr_ConveyorItemMover> boxPool = new List<scr_ConveyorItemMover>();

    [Header("Spawn Settings")]
    [SerializeField] private float moveSpeed = 0.8f;
    [SerializeField] private float spawnInterval = 1.2f;
    [SerializeField] private float spawnCheckRadius = 1.0f;
    [SerializeField] private LayerMask boxLayerMask;
    [SerializeField] private bool autoStartOnPlay = true;

    private float spawnTimer;
    private readonly List<scr_ConveyorItemMover> activeItems = new List<scr_ConveyorItemMover>();

    private void Start()
    {
        if (autoStartOnPlay)
        {
            InitializePool();
        }
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;
        TrySpawn();
        RecycleReachedItems();
    }

    [ContextMenu("Initialize Pool")]
    public void InitializePool()
    {
        ValidateReferences();
        activeItems.Clear();

        for (int i = 0; i < boxPool.Count; i++)
        {
            scr_ConveyorItemMover item = boxPool[i];
            if (item == null)
            {
                continue;
            }

            item.Initialize(startPoint, endPoint, moveSpeed);
            item.DeactivateItem();
        }

        spawnTimer = 0f;
    }

    private void TrySpawn()
    {
        if (spawnTimer < spawnInterval || !IsSpawnAreaAvailable())
        {
            return;
        }

        scr_ConveyorItemMover nextItem = GetNextInactiveItem();
        if (nextItem == null)
        {
            return;
        }

        nextItem.SetMoveSpeed(moveSpeed);
        nextItem.ActivateAtStartPoint();

        if (!activeItems.Contains(nextItem))
        {
            activeItems.Add(nextItem);
        }

        spawnTimer = 0f;
    }

    private void RecycleReachedItems()
    {
        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            scr_ConveyorItemMover item = activeItems[i];
            if (item == null)
            {
                activeItems.RemoveAt(i);
                continue;
            }

            if (item.IsMoving() && item.HasReachedEndPoint())
            {
                item.DeactivateItem();
                activeItems.RemoveAt(i);
            }
        }
    }

    private scr_ConveyorItemMover GetNextInactiveItem()
    {
        for (int i = 0; i < boxPool.Count; i++)
        {
            scr_ConveyorItemMover item = boxPool[i];
            if (item != null && !item.gameObject.activeSelf)
            {
                return item;
            }
        }

        return null;
    }

    private bool IsSpawnAreaAvailable()
    {
        if (startPoint == null)
        {
            return false;
        }

        Collider[] hits = Physics.OverlapSphere(startPoint.position, spawnCheckRadius, boxLayerMask);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].gameObject.activeInHierarchy)
            {
                return false;
            }
        }

        return true;
    }

    private void ValidateReferences()
    {
        if (startPoint == null)
        {
            Debug.LogWarning($"[{nameof(scr_ConveyorSpawnManager)}] StartPoint is not assigned.");
        }

        if (endPoint == null)
        {
            Debug.LogWarning($"[{nameof(scr_ConveyorSpawnManager)}] EndPoint is not assigned.");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (startPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startPoint.position, spawnCheckRadius);
        }

        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
        }
    }
#endif
}
