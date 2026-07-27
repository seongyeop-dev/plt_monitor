using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상단 시간 표시 전용
/// - 매 프레임이 아닌 지정 주기로 갱신
/// - 포맷을 인스펙터에서 조정 가능
/// </summary>
public class scr_TimeDisplay : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Text txtTime;

    [Header("Display")]
    [SerializeField] private float refreshInterval = 1.0f;
    [SerializeField] private string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    [SerializeField] private bool refreshOnStart = true;

    private void Start()
    {
        if (!refreshOnStart)
        {
            return;
        }

        StartRefreshing();
    }

    private void OnDisable()
    {
        StopRefreshing();
    }

    [ContextMenu("Refresh Time")]
    public void RefreshTime()
    {
        if (txtTime == null)
        {
            return;
        }

        txtTime.text = DateTime.Now.ToString(dateTimeFormat);
    }

    public void StartRefreshing()
    {
        StopRefreshing();
        RefreshTime();
        InvokeRepeating(nameof(RefreshTime), Mathf.Max(0.1f, refreshInterval), Mathf.Max(0.1f, refreshInterval));
    }

    public void StopRefreshing()
    {
        CancelInvoke(nameof(RefreshTime));
    }
}
