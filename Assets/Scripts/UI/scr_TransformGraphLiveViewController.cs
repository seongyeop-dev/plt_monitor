using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Live Conveyor View Controller
/// - Live View Camera / RawImage 연결
/// - 선택 대상 추적
/// - 우측 상태 정보 표시
/// - ConveyorZoomPanel 확대 / 복귀 토글
/// - 정면 / 옆면 / 사선 뷰 프리셋 지원
/// </summary>
public class scr_TransformGraphLiveViewController : MonoBehaviour, IPointerClickHandler
{
    private enum ViewPreset
    {
        Front,
        Side,
        Diagonal
    }

    [Header("Camera")]
    [SerializeField] private Camera liveViewCamera;
    [SerializeField] private RenderTexture liveRenderTexture;

    [Header("UI")]
    [SerializeField] private RawImage liveViewRawImage;
    [SerializeField] private Button liveViewOpenButton;
    [SerializeField] private Text liveSelectedObjectText;
    [SerializeField] private Text liveMetricText;
    [SerializeField] private Text liveStatusText;

    [Header("View Preset Buttons")]
    [SerializeField] private Button frontViewButton;
    [SerializeField] private Button sideViewButton;
    [SerializeField] private Button diagonalViewButton;

    [Header("Follow Target")]
    [SerializeField] private Transform defaultViewTarget;
    [SerializeField] private bool followSelectedTarget = true;
    [SerializeField] private float followLerpSpeed = 5f;

    [Header("Default View Offset")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 5f, -8f);
    [SerializeField] private Vector3 lookOffset = Vector3.zero;

    [Header("Preset Offsets")]
    [SerializeField] private Vector3 frontViewOffset = new Vector3(0f, 5f, -8f);
    [SerializeField] private Vector3 frontLookOffset = Vector3.zero;
    [SerializeField] private Vector3 sideViewOffset = new Vector3(8f, 3.5f, 0f);
    [SerializeField] private Vector3 sideLookOffset = Vector3.zero;
    [SerializeField] private Vector3 diagonalViewOffset = new Vector3(7f, 5f, -7f);
    [SerializeField] private Vector3 diagonalLookOffset = Vector3.zero;

    [Header("Click Option")]
    [SerializeField] private bool enableClickToResetView = false;
    [SerializeField] private string defaultViewName = "Container View";
    [SerializeField] private string defaultMetricName = "Overview";
    [SerializeField] private string defaultStatusText = "Viewing Container";

    [Header("Dashboard")]
    [SerializeField] private GameObject dashboardPanel;
    [SerializeField] private GameObject focusGraphArea;
    [SerializeField] private Button closeDashboardButton;
    [SerializeField] private GameObject[] uiRootsToHideWhenDashboardOpen;

    private Transform currentTarget;
    private string currentObjectName = "-";
    private string currentMetricName = "-";
    private string currentStatus = "Ready";

    private Vector3 currentCameraOffset;
    private Vector3 currentLookOffset;
    private ViewPreset currentViewPreset = ViewPreset.Front;

    private void Start()
    {
        InitializeLiveView();
        BindDashboardButtons();
        ApplyViewPreset(ViewPreset.Front);
        SetDashboardVisible(false);
    }

    private void LateUpdate()
    {
        UpdateCameraFollow();
    }

    public void InitializeLiveView()
    {
        if (liveViewCamera == null)
        {
            Debug.LogWarning("[LiveViewController] LiveViewCamera is not assigned.");
            return;
        }

        EnsureRenderTexture();

        liveViewCamera.targetTexture = liveRenderTexture;

        if (liveViewRawImage != null)
        {
            liveViewRawImage.texture = liveRenderTexture;
            liveViewRawImage.raycastTarget = true;
        }

        ResetToDefaultView();
    }

    public void SetSelectedTarget(Transform targetTransform, string objectName)
    {
        currentTarget = ResolveFollowTarget(targetTransform);
        currentObjectName = string.IsNullOrWhiteSpace(objectName) ? defaultViewName : objectName;
        RefreshLiveInfo();
    }

    public void SetMetricInfo(string metricName)
    {
        currentMetricName = string.IsNullOrWhiteSpace(metricName) ? defaultMetricName : metricName;
        RefreshLiveInfo();
    }

    public void SetStatus(string statusText)
    {
        currentStatus = string.IsNullOrWhiteSpace(statusText) ? defaultStatusText : statusText;
        RefreshLiveInfo();
    }

    public void ResetToDefaultView()
    {
        currentTarget = defaultViewTarget;
        currentObjectName = defaultViewName;
        currentMetricName = defaultMetricName;
        currentStatus = defaultStatusText;

        ApplyViewPreset(currentViewPreset);
        RefreshLiveInfo();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!enableClickToResetView)
        {
            return;
        }

        if (liveViewRawImage == null)
        {
            return;
        }

        bool isInside = RectTransformUtility.RectangleContainsScreenPoint(
            liveViewRawImage.rectTransform,
            eventData.position,
            eventData.pressEventCamera);

        if (isInside)
        {
            ResetToDefaultView();
        }
    }

    public void OnLiveViewClicked()
    {
        SetDashboardVisible(true);
    }

    public void CloseDashboard()
    {
        SetDashboardVisible(false);
    }

    public void SetFrontView()
    {
        ApplyViewPreset(ViewPreset.Front);
    }

    public void SetSideView()
    {
        ApplyViewPreset(ViewPreset.Side);
    }

    public void SetDiagonalView()
    {
        ApplyViewPreset(ViewPreset.Diagonal);
    }

    private void ApplyViewPreset(ViewPreset preset)
    {
        currentViewPreset = preset;

        switch (preset)
        {
            case ViewPreset.Front:
                currentCameraOffset = frontViewOffset;
                currentLookOffset = frontLookOffset;
                break;

            case ViewPreset.Side:
                currentCameraOffset = sideViewOffset;
                currentLookOffset = sideLookOffset;
                break;

            case ViewPreset.Diagonal:
                currentCameraOffset = diagonalViewOffset;
                currentLookOffset = diagonalLookOffset;
                break;
        }
    }

    private void SetDashboardVisible(bool isVisible)
    {
        if (dashboardPanel != null)
        {
            dashboardPanel.SetActive(isVisible);

            if (isVisible)
            {
                dashboardPanel.transform.SetAsLastSibling();
            }
        }

        if (focusGraphArea != null)
        {
            focusGraphArea.SetActive(!isVisible);
        }

        if (uiRootsToHideWhenDashboardOpen != null)
        {
            for (int i = 0; i < uiRootsToHideWhenDashboardOpen.Length; i++)
            {
                GameObject target = uiRootsToHideWhenDashboardOpen[i];

                if (target == null)
                {
                    continue;
                }

                target.SetActive(!isVisible);
            }
        }
    }

    private void BindDashboardButtons()
    {
        BindButton(liveViewOpenButton, OnLiveViewClicked);
        BindButton(closeDashboardButton, CloseDashboard);
        BindButton(frontViewButton, SetFrontView);
        BindButton(sideViewButton, SetSideView);
        BindButton(diagonalViewButton, SetDiagonalView);
    }

    private void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.transition = Selectable.Transition.None;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private Transform ResolveFollowTarget(Transform selectedTarget)
    {
        if (selectedTarget != null && followSelectedTarget)
        {
            return selectedTarget;
        }

        return defaultViewTarget;
    }

    private void EnsureRenderTexture()
    {
        if (liveRenderTexture != null)
        {
            return;
        }

        liveRenderTexture = new RenderTexture(512, 256, 16)
        {
            name = "RT_GraphLiveView"
        };
    }

    private void UpdateCameraFollow()
    {
        if (liveViewCamera == null || currentTarget == null)
        {
            return;
        }

        Vector3 desiredPosition = currentTarget.position + currentCameraOffset;
        liveViewCamera.transform.position = Vector3.Lerp(
            liveViewCamera.transform.position,
            desiredPosition,
            Time.deltaTime * followLerpSpeed);

        Vector3 lookPosition = currentTarget.position + currentLookOffset;
        Vector3 lookDirection = lookPosition - liveViewCamera.transform.position;

        if (lookDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection);
        liveViewCamera.transform.rotation = Quaternion.Lerp(
            liveViewCamera.transform.rotation,
            desiredRotation,
            Time.deltaTime * followLerpSpeed);
    }

    private void RefreshLiveInfo()
    {
        if (liveSelectedObjectText != null)
        {
            liveSelectedObjectText.text = "Selected : " + currentObjectName;
        }

        if (liveMetricText != null)
        {
            liveMetricText.text = "Metric : " + currentMetricName;
        }

        if (liveStatusText != null)
        {
            liveStatusText.text = "Status : " + currentStatus;
        }
    }
}