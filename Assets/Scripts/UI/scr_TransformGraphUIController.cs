using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class scr_TransformGraphUIController : MonoBehaviour
{
    public enum GraphMode
    {
        Overview,
        Detail,
        Focus
    }

    public enum AxisSelectionMode
    {
        All,
        X,
        Y,
        Z
    }

    [Header("Core References")]
    [SerializeField] private scr_TransformGraphDataProvider graphDataProvider;
    [SerializeField] private scr_TransformGraphRenderer graphRenderer;
    [SerializeField] private scr_TransformGraphLiveViewController liveViewController;

    [Header("Graph Area Labels")]
    [SerializeField] private Text axisLabelX;
    [SerializeField] private Text axisLabelY;
    [SerializeField] private Text legendText;

    [Header("Object UI")]
    [SerializeField] private RectTransform objectButtonContainer;
    [SerializeField] private Button objectButtonPrefab;

    [Header("Mode Buttons")]
    [SerializeField] private Button overviewButton;
    [SerializeField] private Button detailButton;
    [SerializeField] private Button focusButton;

    [Header("Category Buttons")]
    [SerializeField] private Button positionButton;
    [SerializeField] private Button rotationButton;
    [SerializeField] private Button scaleButton;

    [Header("Axis Buttons")]
    [SerializeField] private Button allButton;
    [SerializeField] private Button axisXButton;
    [SerializeField] private Button axisYButton;
    [SerializeField] private Button axisZButton;

    [Header("Selection Button Visual")]
    [SerializeField] private Color buttonActiveColor = new Color32(15, 72, 140, 255);
    [SerializeField] private Color buttonInactiveColor = new Color32(28, 63, 104, 255);
    [SerializeField] private Color buttonActiveTextColor = Color.white;
    [SerializeField] private Color buttonInactiveTextColor = Color.white;

    [Header("Summary Label")]
    [SerializeField] private Text summaryText;

    [Header("Graph Info Labels")]
    [SerializeField] private Text metricTitleText;
    [SerializeField] private Text currentValueText;
    [SerializeField] private Text minValueText;
    [SerializeField] private Text maxValueText;
    [SerializeField] private Text frameRangeText;

    [Header("Interpretation Labels")]
    [SerializeField] private Text conveyorStateText;
    [SerializeField] private Text axisMeaningText;
    [SerializeField] private Text riskText;
    [SerializeField] private Text insightText;

    [Header("Default Selection")]
    [SerializeField] private string defaultObjectName = "Box_01";
    [SerializeField]
    private scr_TransformGraphDataProvider.TransformCategory defaultCategory =
        scr_TransformGraphDataProvider.TransformCategory.Position;
    [SerializeField]
    private scr_TransformGraphDataProvider.AxisType defaultAxis =
        scr_TransformGraphDataProvider.AxisType.Z;
    [SerializeField] private GraphMode defaultMode = GraphMode.Overview;

    [Header("View Panels")]
    [SerializeField] private GameObject overviewPanel;
    [SerializeField] private GameObject detailQuadPanel;
    [SerializeField] private GameObject focusGraphArea;
    [SerializeField] private scr_TransformGraphOverviewController overviewController;
    [SerializeField] private scr_TransformGraphDetailQuadController detailQuadController;

    [Header("Run Option")]
    [SerializeField] private bool initializeOnStart = true;

    [Header("Sliding Window UI")]
    [SerializeField] private int visiblePointCount = 120;

    private readonly List<Button> spawnedObjectButtons = new List<Button>();

    private string currentObjectName;
    private scr_TransformGraphDataProvider.TransformCategory currentCategory;
    private scr_TransformGraphDataProvider.AxisType currentAxis;
    private GraphMode currentMode;
    private AxisSelectionMode currentAxisSelectionMode = AxisSelectionMode.All;

    private void Start()
    {
        if (!initializeOnStart)
        {
            return;
        }

        InitializeUI();
    }

    [ContextMenu("Initialize UI")]
    public void InitializeUI()
    {
        if (!ValidateCoreReferences())
        {
            return;
        }

        BindStaticButtons();
        RebuildObjectButtons();
        ApplyDefaultSelection();
        BindOverviewController();

        UpdateModePanels();
        ApplyButtonTransitionNone();
        RefreshGraph();
        UpdateSelectionButtonVisuals();
    }

    [ContextMenu("Refresh Graph")]
    public void RefreshGraph()
    {
        if (!ValidateCoreReferences())
        {
            return;
        }

        if (!EnsureCurrentObjectSelected())
        {
            graphRenderer.ClearGraph();
            detailQuadController?.ClearAll();
            UpdateSummaryText();
            UpdateSelectionButtonVisuals();
            return;
        }

        switch (currentMode)
        {
            case GraphMode.Overview:
                RenderOverview();
                break;

            case GraphMode.Detail:
                RenderDetail();
                break;

            case GraphMode.Focus:
                RenderFocus();
                break;
        }

        SyncOverviewSelection();
        UpdateSummaryText();
        UpdateAxisLabels();
        UpdateLegend();
        UpdateLiveView();
        UpdateSelectionButtonVisuals();
    }

    [ContextMenu("Rebuild Object Buttons")]
    public void RebuildObjectButtons()
    {
        ClearObjectButtons();

        if (graphDataProvider == null || objectButtonContainer == null || objectButtonPrefab == null)
        {
            Debug.LogWarning("[GraphUIController] Missing object button reference.");
            return;
        }

        List<string> objectNames = graphDataProvider.GetObjectNameList();

        for (int i = 0; i < objectNames.Count; i++)
        {
            string objectName = objectNames[i];
            Button buttonInstance = Instantiate(objectButtonPrefab, objectButtonContainer);
            buttonInstance.gameObject.name = $"Btn_{objectName}";

            SetButtonLabel(buttonInstance, objectName);
            SetButtonTransitionNone(buttonInstance);

            string capturedName = objectName;
            buttonInstance.onClick.RemoveAllListeners();
            buttonInstance.onClick.AddListener(() => SelectObjectFromList(capturedName, true));

            spawnedObjectButtons.Add(buttonInstance);
        }

        Debug.Log($"[GraphUIController] Object button count = {spawnedObjectButtons.Count}");
    }

    private bool ValidateCoreReferences()
    {
        if (graphDataProvider == null)
        {
            Debug.LogWarning("[GraphUIController] GraphDataProvider is not assigned.");
            return false;
        }

        if (graphRenderer == null)
        {
            Debug.LogWarning("[GraphUIController] GraphRenderer is not assigned.");
            return false;
        }

        return true;
    }

    private void ApplyDefaultSelection()
    {
        currentCategory = defaultCategory;
        currentAxis = defaultAxis;
        currentMode = defaultMode;
        currentAxisSelectionMode = AxisSelectionMode.All;

        if (graphDataProvider.HasRecorder(defaultObjectName))
        {
            currentObjectName = defaultObjectName;
            return;
        }

        List<string> objectNames = graphDataProvider.GetObjectNameList();
        currentObjectName = objectNames.Count > 0 ? objectNames[0] : string.Empty;
    }

    private void BindOverviewController()
    {
        if (overviewController == null)
        {
            return;
        }

        overviewController.OnOverviewCardClicked -= HandleOverviewCardClicked;
        overviewController.OnOverviewCardClicked += HandleOverviewCardClicked;
        overviewController.BuildCards();
        overviewController.SetSelectedCard(currentObjectName);
    }

    private bool EnsureCurrentObjectSelected()
    {
        if (!string.IsNullOrWhiteSpace(currentObjectName) && graphDataProvider.HasRecorder(currentObjectName))
        {
            return true;
        }

        List<string> objectNames = graphDataProvider.GetObjectNameList();
        currentObjectName = objectNames.Count > 0 ? objectNames[0] : string.Empty;
        return !string.IsNullOrWhiteSpace(currentObjectName);
    }

    private void RenderOverview()
    {
        if (overviewController == null)
        {
            return;
        }

        scr_TransformGraphDataProvider.AxisType axisForOverview = GetOverviewAxis();

        overviewController.RefreshCards(currentCategory, axisForOverview);
        UpdateGraphInfoOverviewForCards();
        UpdateInterpretationOverview(axisForOverview);
        detailQuadController?.ClearAll();
    }

    private void RenderDetail()
    {
        Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>> seriesMap = BuildDetailSeriesMap();

        if (detailQuadController != null)
        {
            detailQuadController.RenderDetail(currentObjectName, currentCategory, seriesMap);
        }

        UpdateGraphInfoDetail(seriesMap);
        UpdateInterpretationDetail(seriesMap);
        graphRenderer.ClearGraph();
    }

    private void RenderFocus()
    {
        List<scr_TransformGraphDataProvider.GraphPoint> points =
            graphDataProvider.GetGraphPoints(currentObjectName, currentCategory, currentAxis);

        if (points == null || points.Count == 0)
        {
            graphRenderer.ClearGraph();
            return;
        }

        ApplyFocusAxisSettings();

        graphRenderer.RenderSingle(points);
        UpdateGraphInfoFocus(points);
        UpdateInterpretationFocus(points);
        detailQuadController?.ClearAll();
    }

    private void ApplyFocusAxisSettings()
    {
        if (graphRenderer == null)
        {
            return;
        }

        graphRenderer.SetYLabelLayout(
            newLeftPadding: 92f,
            newOutsideOffset: 8f,
            newLabelSize: new Vector2(78f, 20f));

        switch (currentCategory)
        {
            case scr_TransformGraphDataProvider.TransformCategory.Position:
                graphRenderer.SetYLabelRule(true, 4, 2);

                switch (currentAxis)
                {
                    case scr_TransformGraphDataProvider.AxisType.X:
                        graphRenderer.SetFixedYRange(true, -2f, 2f);
                        break;

                    case scr_TransformGraphDataProvider.AxisType.Y:
                        graphRenderer.SetFixedYRange(true, 0f, 2f);
                        break;

                    case scr_TransformGraphDataProvider.AxisType.Z:
                        graphRenderer.SetFixedYRange(true, -6f, 12f);
                        break;
                }
                break;

            case scr_TransformGraphDataProvider.TransformCategory.Rotation:
                graphRenderer.SetYLabelRule(true, 5, 90);
                graphRenderer.SetFixedYRange(true, 0f, 360f);
                break;

            case scr_TransformGraphDataProvider.TransformCategory.Scale:
                graphRenderer.SetYLabelRule(true, 3, 1);
                graphRenderer.SetFixedYRange(true, 0f, 2f);
                break;

            default:
                graphRenderer.SetFixedYRange(false, 0f, 0f);
                break;
        }
    }

    private Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>> BuildDetailSeriesMap()
    {
        Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>> seriesMap =
            new Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>>();

        AddSeriesIfAvailable(seriesMap, "X", scr_TransformGraphDataProvider.AxisType.X);
        AddSeriesIfAvailable(seriesMap, "Y", scr_TransformGraphDataProvider.AxisType.Y);
        AddSeriesIfAvailable(seriesMap, "Z", scr_TransformGraphDataProvider.AxisType.Z);

        return seriesMap;
    }

    private void AddSeriesIfAvailable(
        Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>> seriesMap,
        string key,
        scr_TransformGraphDataProvider.AxisType axis)
    {
        List<scr_TransformGraphDataProvider.GraphPoint> points =
            graphDataProvider.GetGraphPoints(currentObjectName, currentCategory, axis);

        if (points != null && points.Count > 0)
        {
            seriesMap[key] = points;
        }
    }

    private void BindStaticButtons()
    {
        BindButton(positionButton, () => SetCategory(scr_TransformGraphDataProvider.TransformCategory.Position));
        BindButton(rotationButton, () => SetCategory(scr_TransformGraphDataProvider.TransformCategory.Rotation));
        BindButton(scaleButton, () => SetCategory(scr_TransformGraphDataProvider.TransformCategory.Scale));

        BindButton(allButton, () => SetAxisMode(AxisSelectionMode.All, currentAxis));
        BindButton(axisXButton, () => SetAxisMode(AxisSelectionMode.X, scr_TransformGraphDataProvider.AxisType.X));
        BindButton(axisYButton, () => SetAxisMode(AxisSelectionMode.Y, scr_TransformGraphDataProvider.AxisType.Y));
        BindButton(axisZButton, () => SetAxisMode(AxisSelectionMode.Z, scr_TransformGraphDataProvider.AxisType.Z));

        BindButton(overviewButton, () => SetMode(GraphMode.Overview));
        BindButton(detailButton, () => SetMode(GraphMode.Detail));
        BindButton(focusButton, () => SetMode(GraphMode.Focus));
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

    private void SetCategory(scr_TransformGraphDataProvider.TransformCategory category)
    {
        currentCategory = category;
        RefreshGraph();
    }

    private void SetAxisMode(AxisSelectionMode selectionMode, scr_TransformGraphDataProvider.AxisType axis)
    {
        currentAxisSelectionMode = selectionMode;
        currentAxis = axis;
        RefreshGraph();
    }

    private void SetMode(GraphMode mode)
    {
        currentMode = mode;

        if (currentMode == GraphMode.Detail)
        {
            currentAxisSelectionMode = AxisSelectionMode.All;
        }
        else if (currentAxisSelectionMode == AxisSelectionMode.All)
        {
            currentAxisSelectionMode = AxisSelectionMode.Z;
            currentAxis = scr_TransformGraphDataProvider.AxisType.Z;
        }

        UpdateModePanels();
        RefreshGraph();
    }

    private void UpdateModePanels()
    {
        bool isOverview = currentMode == GraphMode.Overview;
        bool isDetail = currentMode == GraphMode.Detail;
        bool isFocus = currentMode == GraphMode.Focus;

        if (overviewPanel != null)
        {
            overviewPanel.SetActive(isOverview);
        }

        if (detailQuadPanel != null)
        {
            detailQuadPanel.SetActive(isDetail);
        }

        if (focusGraphArea != null)
        {
            focusGraphArea.SetActive(isFocus);
        }
    }

    private void SyncOverviewSelection()
    {
        overviewController?.SetSelectedCard(currentObjectName);
    }

    private void UpdateSummaryText()
    {
        if (summaryText == null)
        {
            return;
        }

        summaryText.text =
            $"Target : {currentObjectName}   Category : {currentCategory}   Axis : {GetSummaryAxisText()}";
    }

    private string GetSummaryAxisText()
    {
        if (currentMode == GraphMode.Detail)
        {
            return "All";
        }

        if (currentMode == GraphMode.Overview && currentAxisSelectionMode == AxisSelectionMode.All)
        {
            return "Z";
        }

        return GetAxisSelectionText();
    }

    private void UpdateGraphInfoOverviewForCards()
    {
        SetGraphInfo(
            "Overview",
            "Current : Compare",
            "Min : -",
            "Max : -",
            "Frame : Recent");
    }

    private void UpdateGraphInfoDetail(Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>> seriesMap)
    {
        if (seriesMap == null || seriesMap.Count == 0)
        {
            return;
        }

        GetSeriesStats(seriesMap, out float globalMin, out float globalMax, out int minFrame, out int maxFrame);

        SetGraphInfo(
            "Detail 4-Panel",
            "Current : X / Y / Z / All",
            $"Min : {globalMin:F3}",
            $"Max : {globalMax:F3}",
            $"Frame : {minFrame} ~ {maxFrame}");
    }

    private void UpdateGraphInfoFocus(List<scr_TransformGraphDataProvider.GraphPoint> points)
    {
        if (points == null || points.Count == 0)
        {
            return;
        }

        List<scr_TransformGraphDataProvider.GraphPoint> visiblePoints = GetVisibleWindow(points);
        if (visiblePoints.Count == 0)
        {
            return;
        }

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < visiblePoints.Count; i++)
        {
            float value = visiblePoints[i].value;
            if (value < minValue) minValue = value;
            if (value > maxValue) maxValue = value;
        }

        float currentValue = visiblePoints[visiblePoints.Count - 1].value;
        int minFrame = visiblePoints[0].frameIndex;
        int maxFrame = visiblePoints[visiblePoints.Count - 1].frameIndex;

        SetGraphInfo(
            "Focus",
            $"Current : {currentValue:F3}",
            $"Min : {minValue:F3}",
            $"Max : {maxValue:F3}",
            $"Frame : {minFrame} ~ {maxFrame}");
    }

    private void SetGraphInfo(string title, string current, string min, string max, string frame)
    {
        if (metricTitleText != null) metricTitleText.text = title;
        if (currentValueText != null) currentValueText.text = current;
        if (minValueText != null) minValueText.text = min;
        if (maxValueText != null) maxValueText.text = max;
        if (frameRangeText != null) frameRangeText.text = frame;
    }

    private void GetSeriesStats(
        Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>> seriesMap,
        out float globalMin,
        out float globalMax,
        out int minFrame,
        out int maxFrame)
    {
        globalMin = float.MaxValue;
        globalMax = float.MinValue;
        minFrame = int.MaxValue;
        maxFrame = int.MinValue;

        foreach (var pair in seriesMap)
        {
            List<scr_TransformGraphDataProvider.GraphPoint> visiblePoints = GetVisibleWindow(pair.Value);
            if (visiblePoints.Count == 0)
            {
                continue;
            }

            for (int i = 0; i < visiblePoints.Count; i++)
            {
                float value = visiblePoints[i].value;
                if (value < globalMin) globalMin = value;
                if (value > globalMax) globalMax = value;
            }

            if (visiblePoints[0].frameIndex < minFrame)
            {
                minFrame = visiblePoints[0].frameIndex;
            }

            if (visiblePoints[visiblePoints.Count - 1].frameIndex > maxFrame)
            {
                maxFrame = visiblePoints[visiblePoints.Count - 1].frameIndex;
            }
        }
    }

    private void UpdateInterpretationOverview(scr_TransformGraphDataProvider.AxisType axisForOverview)
    {
        List<scr_TransformGraphDataProvider.GraphPoint> points =
            graphDataProvider.GetGraphPoints(currentObjectName, currentCategory, axisForOverview);

        scr_TransformGraphMeaningAnalyzer.AxisAnalysisResult analysis =
            scr_TransformGraphMeaningAnalyzer.AnalyzeAxis(points, currentCategory, axisForOverview);

        SetInterpretationTexts(
            "Overview 비교 중",
            analysis.meaning,
            analysis.risk,
            "각 카드의 상태를 비교해 병목, 편차, 흔들림 여부를 빠르게 확인할 수 있습니다.");
    }

    private void UpdateInterpretationDetail(Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>> seriesMap)
    {
        scr_TransformGraphMeaningAnalyzer.DetailAnalysisResult detailResult =
            scr_TransformGraphMeaningAnalyzer.AnalyzeDetail(seriesMap, currentCategory);

        SetInterpretationTexts(
            detailResult.conveyorState,
            detailResult.axisMeaning,
            detailResult.risk,
            detailResult.insight);
    }

    private void UpdateInterpretationFocus(List<scr_TransformGraphDataProvider.GraphPoint> points)
    {
        scr_TransformGraphMeaningAnalyzer.AxisAnalysisResult analysis =
            scr_TransformGraphMeaningAnalyzer.AnalyzeAxis(points, currentCategory, currentAxis);

        SetInterpretationTexts(
            analysis.shortState,
            analysis.meaning,
            analysis.risk,
            analysis.insight);
    }

    private void SetInterpretationTexts(string state, string meaning, string risk, string insight)
    {
        if (conveyorStateText != null)
        {
            conveyorStateText.text = state;
            conveyorStateText.color = Color.white;
        }

        if (axisMeaningText != null)
        {
            axisMeaningText.text = meaning;
            axisMeaningText.color = Color.white;
        }

        if (riskText != null)
        {
            riskText.text = risk;
            riskText.color = GetRiskColor(risk);
        }

        if (insightText != null)
        {
            insightText.text = insight;
            insightText.color = Color.white;
        }
    }

    private Color GetRiskColor(string risk)
    {
        if (string.IsNullOrWhiteSpace(risk))
        {
            return Color.white;
        }

        string normalized = risk.Trim();

        if (normalized.Contains("낮음"))
        {
            return new Color32(86, 227, 159, 255);
        }

        if (normalized.Contains("주의") || normalized.Contains("중간"))
        {
            return new Color32(255, 209, 102, 255);
        }

        if (normalized.Contains("높음") || normalized.Contains("위험"))
        {
            return new Color32(255, 90, 90, 255);
        }

        return Color.white;
    }

    private void UpdateAxisLabels()
    {
        if (axisLabelX != null)
        {
            axisLabelX.text = string.Empty;
        }

        if (axisLabelY != null)
        {
            axisLabelY.text = string.Empty;
        }
    }

    private void UpdateLegend()
    {
        if (legendText == null)
        {
            return;
        }

        legendText.supportRichText = true;

        switch (currentMode)
        {
            case GraphMode.Overview:
                legendText.text = $"<b>Overview</b> - {GetShortCategoryName(currentCategory)} {GetOverviewAxisText()}";
                break;

            case GraphMode.Detail:
                legendText.text =
                    $"{currentObjectName} - {GetShortCategoryName(currentCategory)} " +
                    "<color=#FF5A5A><b>X</b></color> / " +
                    "<color=#59FF8D><b>Y</b></color> / " +
                    "<color=#33D7FF><b>Z</b></color> / " +
                    "<b>All</b>";
                break;

            case GraphMode.Focus:
                legendText.text =
                    $"{currentObjectName} - {GetShortCategoryName(currentCategory)} " +
                    $"<color=#33D7FF><b>{currentAxis}</b></color>";
                break;
        }
    }

    private void UpdateLiveView()
    {
        if (liveViewController == null)
        {
            return;
        }

        Transform selectedTransform = FindSelectedObjectTransform();
        liveViewController.SetSelectedTarget(selectedTransform, currentObjectName);
        liveViewController.SetMetricInfo(BuildLiveMetricText());
        liveViewController.SetStatus("Recording");
    }

    private string BuildLiveMetricText()
    {
        if (currentMode == GraphMode.Detail)
        {
            return GetShortCategoryName(currentCategory) + " XYZ";
        }

        if (currentMode == GraphMode.Overview)
        {
            return GetShortCategoryName(currentCategory) + " " + GetOverviewAxisText();
        }

        return GetShortCategoryName(currentCategory) + " " + GetAxisSelectionText();
    }

    private Transform FindSelectedObjectTransform()
    {
        if (string.IsNullOrWhiteSpace(currentObjectName))
        {
            return null;
        }

        GameObject targetObject = GameObject.Find(currentObjectName);
        return targetObject != null ? targetObject.transform : null;
    }

    private List<scr_TransformGraphDataProvider.GraphPoint> GetVisibleWindow(
        List<scr_TransformGraphDataProvider.GraphPoint> points)
    {
        if (points == null || points.Count == 0)
        {
            return new List<scr_TransformGraphDataProvider.GraphPoint>();
        }

        int count = Mathf.Max(2, visiblePointCount);
        if (points.Count <= count)
        {
            return new List<scr_TransformGraphDataProvider.GraphPoint>(points);
        }

        int startIndex = points.Count - count;
        return points.GetRange(startIndex, count);
    }

    private string GetAxisSelectionText()
    {
        return currentAxisSelectionMode switch
        {
            AxisSelectionMode.All => "All",
            AxisSelectionMode.X => "X",
            AxisSelectionMode.Y => "Y",
            AxisSelectionMode.Z => "Z",
            _ => "-"
        };
    }

    private scr_TransformGraphDataProvider.AxisType GetOverviewAxis()
    {
        if (currentAxisSelectionMode == AxisSelectionMode.All)
        {
            return scr_TransformGraphDataProvider.AxisType.Z;
        }

        return currentAxis;
    }

    private string GetOverviewAxisText()
    {
        return currentAxisSelectionMode == AxisSelectionMode.All ? "Z" : GetAxisSelectionText();
    }

    private string GetShortCategoryName(scr_TransformGraphDataProvider.TransformCategory category)
    {
        return category switch
        {
            scr_TransformGraphDataProvider.TransformCategory.Position => "Pos",
            scr_TransformGraphDataProvider.TransformCategory.Rotation => "Rot",
            scr_TransformGraphDataProvider.TransformCategory.Scale => "Scale",
            _ => category.ToString()
        };
    }

    private void SelectObjectFromList(string objectName, bool switchToDetailMode)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return;
        }

        currentObjectName = objectName;

        if (switchToDetailMode && currentMode == GraphMode.Overview)
        {
            currentMode = GraphMode.Detail;
            currentAxisSelectionMode = AxisSelectionMode.All;
            UpdateModePanels();
        }

        RefreshGraph();
    }

    private void HandleOverviewCardClicked(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return;
        }

        currentObjectName = objectName;
        currentMode = GraphMode.Detail;
        currentAxisSelectionMode = AxisSelectionMode.All;

        UpdateModePanels();
        RefreshGraph();
    }

    private void SetButtonLabel(Button button, string labelText)
    {
        if (button == null)
        {
            return;
        }

        Text label = button.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.text = labelText;
        }
    }

    private void ClearObjectButtons()
    {
        for (int i = 0; i < spawnedObjectButtons.Count; i++)
        {
            if (spawnedObjectButtons[i] != null)
            {
                Destroy(spawnedObjectButtons[i].gameObject);
            }
        }

        spawnedObjectButtons.Clear();
    }

    private void ApplyButtonTransitionNone()
    {
        SetButtonTransitionNone(overviewButton);
        SetButtonTransitionNone(detailButton);
        SetButtonTransitionNone(focusButton);

        SetButtonTransitionNone(positionButton);
        SetButtonTransitionNone(rotationButton);
        SetButtonTransitionNone(scaleButton);

        SetButtonTransitionNone(allButton);
        SetButtonTransitionNone(axisXButton);
        SetButtonTransitionNone(axisYButton);
        SetButtonTransitionNone(axisZButton);
    }

    private void SetButtonTransitionNone(Button button)
    {
        if (button == null)
        {
            return;
        }

        button.transition = Selectable.Transition.None;
    }

    private void UpdateSelectionButtonVisuals()
    {
        SetSelectableButtonVisual(overviewButton, currentMode == GraphMode.Overview);
        SetSelectableButtonVisual(detailButton, currentMode == GraphMode.Detail);
        SetSelectableButtonVisual(focusButton, currentMode == GraphMode.Focus);

        SetSelectableButtonVisual(
            positionButton,
            currentCategory == scr_TransformGraphDataProvider.TransformCategory.Position);

        SetSelectableButtonVisual(
            rotationButton,
            currentCategory == scr_TransformGraphDataProvider.TransformCategory.Rotation);

        SetSelectableButtonVisual(
            scaleButton,
            currentCategory == scr_TransformGraphDataProvider.TransformCategory.Scale);

        SetSelectableButtonVisual(allButton, currentAxisSelectionMode == AxisSelectionMode.All);
        SetSelectableButtonVisual(axisXButton, currentAxisSelectionMode == AxisSelectionMode.X);
        SetSelectableButtonVisual(axisYButton, currentAxisSelectionMode == AxisSelectionMode.Y);
        SetSelectableButtonVisual(axisZButton, currentAxisSelectionMode == AxisSelectionMode.Z);
    }

    private void SetSelectableButtonVisual(Button button, bool isActive)
    {
        if (button == null)
        {
            return;
        }

        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = isActive ? buttonActiveColor : buttonInactiveColor;
        }

        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.color = isActive ? buttonActiveTextColor : buttonInactiveTextColor;
        }
    }
}