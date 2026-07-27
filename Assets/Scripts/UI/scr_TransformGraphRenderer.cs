using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Transform 그래프 렌더러
/// - 단일 / 다중 시리즈 렌더링 지원
/// - Y라벨 표시 옵션 인스펙터 조정 지원
/// - 고정 Y 범위 옵션 지원
/// </summary>
public class scr_TransformGraphRenderer : MonoBehaviour
{
    [Header("Graph Root")]
    [SerializeField] private RectTransform graphArea;
    [SerializeField] private RectTransform gridContainer;
    [SerializeField] private RectTransform pointContainer;
    [SerializeField] private RectTransform lineContainer;

    [Header("Axis Labels")]
    [SerializeField] private RectTransform yLabelContainer;
    [SerializeField] private Font labelFont;
    [SerializeField] private int yLabelCount = 4;
    [SerializeField] private Color labelColor = Color.white;

    [Header("Y Label Display")]
    [SerializeField] private bool useEvenYLabels = true;
    [SerializeField] private int yLabelEvenStep = 2;
    [SerializeField] private bool useIntegerYLabels = true;
    [SerializeField] private string yLabelNumberFormat = "F1";
    [SerializeField] private int yLabelFontSize = 11;
    [SerializeField] private float yLabelOutsideOffset = 12f;
    [SerializeField] private Vector2 yLabelSize = new Vector2(60f, 20f);

    [Header("Point Settings")]
    [SerializeField] private Sprite pointSprite;
    [SerializeField] private Vector2 pointSize = new Vector2(5f, 5f);
    [SerializeField] private bool showPoints = false;

    [Header("Line Settings")]
    [SerializeField] private float lineThickness = 3f;
    [SerializeField] private Color lineColor = new Color(0.16f, 0.83f, 1f, 1f);
    [SerializeField] private bool stepGraph = false;
    [SerializeField] private float lineOverlap = 2.5f;

    [Header("Grid Settings")]
    [SerializeField] private int horizontalGridCount = 4;
    [SerializeField] private int verticalGridCount = 6;
    [SerializeField] private float gridThickness = 1f;
    [SerializeField] private Color gridColor = new Color(0.30f, 0.50f, 0.70f, 0.20f);

    [Header("Graph Padding")]
    [SerializeField] private float leftPadding = 50f;
    [SerializeField] private float rightPadding = 20f;
    [SerializeField] private float topPadding = 30f;
    [SerializeField] private float bottomPadding = 35f;

    [Header("Sliding Window")]
    [SerializeField] private bool useSlidingWindow = true;
    [SerializeField] private int visiblePointCount = 120;

    [Header("Fixed Y Range")]
    [SerializeField] private bool useFixedYRange = false;
    [SerializeField] private float fixedYMin = 0f;
    [SerializeField] private float fixedYMax = 10f;

    private readonly List<GameObject> spawnedPointObjects = new List<GameObject>();
    private readonly List<GameObject> spawnedLineObjects = new List<GameObject>();
    private readonly List<GameObject> spawnedGridObjects = new List<GameObject>();

    public void RenderGraph(List<scr_TransformGraphDataProvider.GraphPoint> graphPoints)
    {
        RenderSingle(graphPoints, lineColor);
    }

    public void RenderSingle(List<scr_TransformGraphDataProvider.GraphPoint> graphPoints)
    {
        RenderSingle(graphPoints, lineColor);
    }

    public void RenderSingle(
        List<scr_TransformGraphDataProvider.GraphPoint> graphPoints,
        Color seriesColor)
    {
        ClearGraph();

        if (!HasGraphReference() || graphPoints == null || graphPoints.Count == 0)
        {
            return;
        }

        List<scr_TransformGraphDataProvider.GraphPoint> visiblePoints = GetVisiblePoints(graphPoints);
        if (visiblePoints.Count == 0)
        {
            return;
        }

        float minValue;
        float maxValue;

        if (useFixedYRange)
        {
            minValue = fixedYMin;
            maxValue = fixedYMax;
        }
        else
        {
            CalculateMinMax(visiblePoints, out minValue, out maxValue);
        }

        DrawGrid();
        DrawYLabels(minValue, maxValue);
        DrawSeries(visiblePoints, minValue, maxValue, seriesColor);
    }

    public void RenderMultiSeries(
        Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>> seriesMap)
    {
        ClearGraph();

        if (!HasGraphReference() || seriesMap == null || seriesMap.Count == 0)
        {
            return;
        }

        Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>> visibleMap =
            BuildVisibleSeriesMap(seriesMap);

        if (visibleMap.Count == 0)
        {
            return;
        }

        float globalMin;
        float globalMax;

        if (useFixedYRange)
        {
            globalMin = fixedYMin;
            globalMax = fixedYMax;
        }
        else
        {
            GetGlobalMinMax(visibleMap, out globalMin, out globalMax);
        }

        DrawGrid();
        DrawYLabels(globalMin, globalMax);

        int colorIndex = 0;
        foreach (var pair in visibleMap)
        {
            DrawSeries(pair.Value, globalMin, globalMax, GetSeriesColor(colorIndex));
            colorIndex++;
        }
    }

    public void ClearGraph()
    {
        DestroySpawnedObjects(spawnedPointObjects);
        DestroySpawnedObjects(spawnedLineObjects);
        DestroySpawnedObjects(spawnedGridObjects);

        if (yLabelContainer != null)
        {
            foreach (Transform child in yLabelContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void CopyVisualSettingsFrom(scr_TransformGraphRenderer source)
    {
        if (source == null)
        {
            return;
        }

        yLabelCount = source.yLabelCount;
        labelColor = source.labelColor;
        useEvenYLabels = source.useEvenYLabels;
        yLabelEvenStep = source.yLabelEvenStep;
        useIntegerYLabels = source.useIntegerYLabels;
        yLabelNumberFormat = source.yLabelNumberFormat;
        yLabelFontSize = source.yLabelFontSize;
        yLabelOutsideOffset = source.yLabelOutsideOffset;
        yLabelSize = source.yLabelSize;

        pointSprite = source.pointSprite;
        pointSize = source.pointSize;
        showPoints = source.showPoints;

        lineThickness = source.lineThickness;
        lineColor = source.lineColor;
        stepGraph = source.stepGraph;
        lineOverlap = source.lineOverlap;

        horizontalGridCount = source.horizontalGridCount;
        verticalGridCount = source.verticalGridCount;
        gridThickness = source.gridThickness;
        gridColor = source.gridColor;

        leftPadding = source.leftPadding;
        rightPadding = source.rightPadding;
        topPadding = source.topPadding;
        bottomPadding = source.bottomPadding;

        useSlidingWindow = source.useSlidingWindow;
        visiblePointCount = source.visiblePointCount;

        useFixedYRange = source.useFixedYRange;
        fixedYMin = source.fixedYMin;
        fixedYMax = source.fixedYMax;

        labelFont = source.labelFont;
    }

    public void SetFixedYRange(bool useFixedRange, float minValue, float maxValue)
    {
        useFixedYRange = useFixedRange;
        fixedYMin = minValue;
        fixedYMax = maxValue;
    }

    public void SetYLabelLayout(float newLeftPadding, float newOutsideOffset, Vector2 newLabelSize)
    {
        leftPadding = newLeftPadding;
        yLabelOutsideOffset = newOutsideOffset;
        yLabelSize = newLabelSize;
    }

    private bool HasGraphReference()
    {
        return graphArea != null &&
               gridContainer != null &&
               pointContainer != null &&
               lineContainer != null;
    }

    public void SetYLabelRule(bool useEvenLabels, int newLabelCount, int newEvenStep = 0)
    {

        useEvenYLabels = useEvenLabels;

        if (newLabelCount > 0)
        {
            yLabelCount = newLabelCount;
        }

        if (newEvenStep > 0)
        {
            yLabelEvenStep = newEvenStep;
        }
    }

    private Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>> BuildVisibleSeriesMap(
        Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>> sourceMap)
    {
        Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>> visibleMap =
            new Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>>();

        foreach (var pair in sourceMap)
        {
            List<scr_TransformGraphDataProvider.GraphPoint> visiblePoints = GetVisiblePoints(pair.Value);
            if (visiblePoints != null && visiblePoints.Count > 0)
            {
                visibleMap[pair.Key] = visiblePoints;
            }
        }

        return visibleMap;
    }

    private List<scr_TransformGraphDataProvider.GraphPoint> GetVisiblePoints(
        List<scr_TransformGraphDataProvider.GraphPoint> sourcePoints)
    {
        if (sourcePoints == null || sourcePoints.Count == 0)
        {
            return new List<scr_TransformGraphDataProvider.GraphPoint>();
        }

        if (!useSlidingWindow)
        {
            return new List<scr_TransformGraphDataProvider.GraphPoint>(sourcePoints);
        }

        int count = Mathf.Max(2, visiblePointCount);
        if (sourcePoints.Count <= count)
        {
            return new List<scr_TransformGraphDataProvider.GraphPoint>(sourcePoints);
        }

        return sourcePoints.GetRange(sourcePoints.Count - count, count);
    }

    private void CalculateMinMax(
        List<scr_TransformGraphDataProvider.GraphPoint> points,
        out float minValue,
        out float maxValue)
    {
        minValue = points[0].value;
        maxValue = points[0].value;

        for (int i = 1; i < points.Count; i++)
        {
            float value = points[i].value;
            if (value < minValue) minValue = value;
            if (value > maxValue) maxValue = value;
        }

        ExpandFlatRange(ref minValue, ref maxValue);
    }

    private void GetGlobalMinMax(
        Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>> visibleMap,
        out float globalMin,
        out float globalMax)
    {
        globalMin = float.MaxValue;
        globalMax = float.MinValue;

        foreach (var pair in visibleMap)
        {
            for (int i = 0; i < pair.Value.Count; i++)
            {
                float value = pair.Value[i].value;
                if (value < globalMin) globalMin = value;
                if (value > globalMax) globalMax = value;
            }
        }

        ExpandFlatRange(ref globalMin, ref globalMax);
    }

    private void ExpandFlatRange(ref float minValue, ref float maxValue)
    {
        if (Mathf.Approximately(minValue, maxValue))
        {
            minValue -= 0.5f;
            maxValue += 0.5f;
        }
    }

    private void DrawSeries(
        List<scr_TransformGraphDataProvider.GraphPoint> points,
        float minValue,
        float maxValue,
        Color seriesColor)
    {
        if (points == null || points.Count < 2)
        {
            return;
        }

        float graphWidth = graphArea.rect.width - leftPadding - rightPadding;
        float graphHeight = graphArea.rect.height - topPadding - bottomPadding;
        int lastIndex = Mathf.Max(1, points.Count - 1);

        List<Vector2> positionList = new List<Vector2>();

        for (int i = 0; i < points.Count; i++)
        {
            float normalizedX = (float)i / lastIndex;
            float normalizedY = Mathf.InverseLerp(minValue, maxValue, points[i].value);

            float x = leftPadding + normalizedX * graphWidth;
            float y = bottomPadding + normalizedY * graphHeight;

            Vector2 pointPosition = new Vector2(x, y);
            positionList.Add(pointPosition);

            if (showPoints)
            {
                CreatePoint(pointPosition, seriesColor);
            }
        }

        for (int i = 0; i < positionList.Count - 1; i++)
        {
            if (stepGraph)
            {
                Vector2 start = positionList[i];
                Vector2 end = positionList[i + 1];
                Vector2 mid = new Vector2(end.x, start.y);

                CreateLine(start, mid, seriesColor);
                CreateLine(mid, end, seriesColor);
            }
            else
            {
                CreateLine(positionList[i], positionList[i + 1], seriesColor);
            }
        }
    }

    private void DrawGrid()
    {
        float width = graphArea.rect.width;
        float height = graphArea.rect.height;

        float graphWidth = width - leftPadding - rightPadding;
        float graphHeight = height - topPadding - bottomPadding;

        for (int i = 0; i <= horizontalGridCount; i++)
        {
            float t = horizontalGridCount == 0 ? 0f : (float)i / horizontalGridCount;
            float y = bottomPadding + t * graphHeight;

            CreateGridLine(
                new Vector2(leftPadding, y),
                new Vector2(width - rightPadding, y));
        }

        for (int i = 0; i <= verticalGridCount; i++)
        {
            float t = verticalGridCount == 0 ? 0f : (float)i / verticalGridCount;
            float x = leftPadding + t * graphWidth;

            CreateGridLine(
                new Vector2(x, bottomPadding),
                new Vector2(x, height - topPadding));
        }
    }

    private void DrawYLabels(float minValue, float maxValue)
    {
        if (yLabelContainer == null)
        {
            return;
        }

        float graphHeight = graphArea.rect.height - topPadding - bottomPadding;
        float range = Mathf.Abs(maxValue - minValue);

        if (useEvenYLabels)
        {
            int step = Mathf.Max(1, yLabelEvenStep);
            float displayMin = Mathf.Floor(minValue / step) * step;
            float displayMax = Mathf.Ceil(maxValue / step) * step;

            if (Mathf.Approximately(displayMin, displayMax))
            {
                displayMax = displayMin + step;
            }

            int safetyCounter = 0;

            for (float labelValue = displayMin; labelValue <= displayMax + 0.001f && safetyCounter < 100; labelValue += step)
            {
                float normalizedY = Mathf.InverseLerp(displayMin, displayMax, labelValue);
                float y = bottomPadding + normalizedY * graphHeight;


                CreateYLabel(labelValue, y, step, false);
                safetyCounter++;
            }

            return;
        }

        float labelStep = range / Mathf.Max(1, yLabelCount);

        for (int i = 0; i <= yLabelCount; i++)
        {
            float t = yLabelCount == 0 ? 0f : (float)i / yLabelCount;
            float value = Mathf.Lerp(minValue, maxValue, t);
            float y = bottomPadding + t * graphHeight;

            CreateYLabel(value, y, labelStep, false);
        }
    }

    private void CreateYLabel(float value, float y, float labelStep, bool forceInteger)
    {
        GameObject labelObject = new GameObject("YLabel", typeof(Text));
        labelObject.transform.SetParent(yLabelContainer, false);

        Text labelText = labelObject.GetComponent<Text>();
        labelText.font = labelFont;
        labelText.fontSize = yLabelFontSize;
        labelText.color = labelColor;
        labelText.alignment = TextAnchor.MiddleRight;

        labelText.text = BuildYLabelText(value, labelStep, forceInteger);

        RectTransform rectTransform = labelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 0f);
        rectTransform.anchorMax = new Vector2(1f, 0f);
        rectTransform.pivot = new Vector2(1f, 0.5f);

        float safeOffset = Mathf.Min(
            yLabelOutsideOffset,
            Mathf.Max(0f, leftPadding - yLabelSize.x - 4f));

        rectTransform.anchoredPosition = new Vector2(-safeOffset, y);
        rectTransform.sizeDelta = yLabelSize;
    }

    private string BuildYLabelText(float value, float labelStep, bool forceInteger)
    {
        if (Mathf.Abs(value) < 0.0001f)
        {
            value = 0f;
        }

        if (forceInteger)
        {
            return Mathf.RoundToInt(value).ToString();
        }

        if (useIntegerYLabels && Mathf.Abs(labelStep) >= 1f && Mathf.Abs(value - Mathf.Round(value)) < 0.0001f)
        {
            return Mathf.RoundToInt(value).ToString();
        }

        int decimalCount = GetDecimalCountForStep(labelStep);
        string text = value.ToString($"F{decimalCount}");

        if (text.Contains("."))
        {
            text = text.TrimEnd('0').TrimEnd('.');
        }

        return text;
    }

    private int GetDecimalCountForStep(float step)
    {
        step = Mathf.Abs(step);

        if (step <= 0.0001f)
        {
            return 2;
        }

        int decimalCount = 0;
        float temp = step;

        while (decimalCount < 4 && Mathf.Abs(temp - Mathf.Round(temp)) > 0.0001f)
        {
            temp *= 10f;
            decimalCount++;
        }

        return decimalCount;
    }

    private void CreatePoint(Vector2 position, Color color)
    {
        if (pointContainer == null)
        {
            return;
        }

        GameObject pointObject = new GameObject("Point", typeof(Image));
        pointObject.transform.SetParent(pointContainer, false);

        Image pointImage = pointObject.GetComponent<Image>();
        pointImage.sprite = pointSprite;
        pointImage.color = color;

        RectTransform rectTransform = pointObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = pointSize;
        rectTransform.anchoredPosition = position;

        spawnedPointObjects.Add(pointObject);
    }

    private void CreateLine(Vector2 startPoint, Vector2 endPoint, Color color)
    {
        if (lineContainer == null)
        {
            return;
        }

        GameObject lineObject = new GameObject("Line", typeof(Image));
        lineObject.transform.SetParent(lineContainer, false);

        Image lineImage = lineObject.GetComponent<Image>();
        lineImage.color = color;

        RectTransform rectTransform = lineObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);
        rectTransform.pivot = new Vector2(0f, 0.5f);

        Vector2 direction = (endPoint - startPoint).normalized;
        float distance = Vector2.Distance(startPoint, endPoint);

        if (distance <= 0.0001f)
        {
            Destroy(lineObject);
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float adjustedLength = distance + lineOverlap;

        rectTransform.sizeDelta = new Vector2(adjustedLength, lineThickness);
        rectTransform.anchoredPosition = startPoint - (direction * (lineOverlap * 0.5f));
        rectTransform.localEulerAngles = new Vector3(0f, 0f, angle);

        spawnedLineObjects.Add(lineObject);
    }

    private void CreateGridLine(Vector2 startPoint, Vector2 endPoint)
    {
        if (gridContainer == null)
        {
            return;
        }

        GameObject gridObject = new GameObject("Grid", typeof(Image));
        gridObject.transform.SetParent(gridContainer, false);

        Image gridImage = gridObject.GetComponent<Image>();
        gridImage.color = gridColor;

        RectTransform rectTransform = gridObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);
        rectTransform.pivot = new Vector2(0f, 0.5f);

        Vector2 direction = (endPoint - startPoint).normalized;
        float distance = Vector2.Distance(startPoint, endPoint);

        if (distance <= 0.0001f)
        {
            Destroy(gridObject);
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rectTransform.sizeDelta = new Vector2(distance, gridThickness);
        rectTransform.anchoredPosition = startPoint;
        rectTransform.localEulerAngles = new Vector3(0f, 0f, angle);

        spawnedGridObjects.Add(gridObject);
    }

    private Color GetSeriesColor(int index)
    {
        Color[] colors =
        {
            Color.red,
            Color.green,
            Color.cyan,
            Color.yellow,
            Color.magenta,
            Color.white,
            new Color(1f, 0.5f, 0f),
            new Color(0.5f, 1f, 1f),
            new Color(0.8f, 0.6f, 1f),
            new Color(0.6f, 1f, 0.6f),
            new Color(1f, 0.8f, 0.4f),
            new Color(0.9f, 0.4f, 0.6f),
            new Color(0.4f, 0.8f, 1f),
            new Color(0.7f, 1f, 0.3f),
            new Color(1f, 0.3f, 0.8f),
            new Color(0.9f, 0.9f, 0.9f)
        };

        return colors[index % colors.Length];
    }

    private void DestroySpawnedObjects(List<GameObject> targetList)
    {
        for (int i = 0; i < targetList.Count; i++)
        {
            if (targetList[i] != null)
            {
                Destroy(targetList[i]);
            }
        }

        targetList.Clear();
    }
}