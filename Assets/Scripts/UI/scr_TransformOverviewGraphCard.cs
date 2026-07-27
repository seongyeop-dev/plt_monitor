using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class scr_TransformOverviewGraphCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text currentValueText;
    [SerializeField] private Text stateText;
    [SerializeField] private Text meaningText;

    [Header("Mini Graph")]
    [SerializeField] private RectTransform graphArea;
    [SerializeField] private RectTransform lineContainer;
    [SerializeField] private RectTransform pointContainer;

    [Header("Graph Style")]
    [SerializeField] private float lineThickness = 2f;
    [SerializeField] private Color lineColor = new Color(0.16f, 0.83f, 1f, 1f);
    [SerializeField] private bool showPoints = false;
    [SerializeField] private Vector2 pointSize = new Vector2(4f, 4f);
    [SerializeField] private float lineOverlap = 1.8f;

    [Header("Window")]
    [SerializeField] private int visiblePointCount = 60;
    [SerializeField] private float paddingLeft = 8f;
    [SerializeField] private float paddingRight = 8f;
    [SerializeField] private float paddingTop = 8f;
    [SerializeField] private float paddingBottom = 8f;

    [Header("Card Visual")]
    [SerializeField] private Color normalColor = new Color32(18, 30, 45, 180);
    [SerializeField] private Color hoverColor = new Color32(28, 50, 75, 200);
    [SerializeField] private Color selectedColor = new Color32(45, 79, 110, 220);

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    private Image backgroundImage;
    private Button cardButton;

    private string objectName = string.Empty;
    private bool isSelected;
    private Action<string> clickCallback;

    private void Awake()
    {
        CacheComponents();
        EnsureButton();
        RefreshCardVisual();
    }

    public void Setup(string boxName, Action<string> onClickCallback)
    {
        objectName = boxName;
        clickCallback = onClickCallback;
        SetCardTitle(boxName);
    }

    public void SetCardTitle(string boxName)
    {
        objectName = boxName;

        if (titleText != null)
        {
            titleText.text = boxName;
        }
    }

    public void SetInterpretation(string shortState, string meaning)
    {
        if (stateText != null)
        {
            stateText.text = shortState;
        }

        if (meaningText != null)
        {
            meaningText.text = meaning;
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        RefreshCardVisual();
    }

    public string GetObjectName()
    {
        return objectName;
    }

    public void RenderGraph(List<scr_TransformGraphDataProvider.GraphPoint> points)
    {
        ClearGraph();

        if (points == null || points.Count < 2)
        {
            ApplyEmptyCardState();
            return;
        }

        List<scr_TransformGraphDataProvider.GraphPoint> visiblePoints = GetVisiblePoints(points);
        if (visiblePoints.Count < 2)
        {
            ApplyEmptyCardState();
            return;
        }

        GetMinMax(visiblePoints, out float minValue, out float maxValue);

        float width = graphArea.rect.width - paddingLeft - paddingRight;
        float height = graphArea.rect.height - paddingTop - paddingBottom;
        int lastIndex = Mathf.Max(1, visiblePoints.Count - 1);

        List<Vector2> pointPositions = new List<Vector2>(visiblePoints.Count);

        for (int i = 0; i < visiblePoints.Count; i++)
        {
            float normalizedX = (float)i / lastIndex;
            float normalizedY = Mathf.InverseLerp(minValue, maxValue, visiblePoints[i].value);

            Vector2 position = new Vector2(
                paddingLeft + normalizedX * width,
                paddingBottom + normalizedY * height);

            pointPositions.Add(position);

            if (showPoints)
            {
                CreatePoint(position);
            }
        }

        for (int i = 0; i < pointPositions.Count - 1; i++)
        {
            CreateLine(pointPositions[i], pointPositions[i + 1]);
        }

        if (currentValueText != null)
        {
            currentValueText.text = $"Cur: {visiblePoints[visiblePoints.Count - 1].value:F3}";
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected && backgroundImage != null)
        {
            backgroundImage.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RefreshCardVisual();
    }

    private void CacheComponents()
    {
        backgroundImage = GetComponent<Image>();
    }

    private void EnsureButton()
    {
        cardButton = GetComponent<Button>();
        if (cardButton == null)
        {
            cardButton = gameObject.AddComponent<Button>();
        }

        cardButton.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = cardButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = Color.white;
        colors.selectedColor = Color.white;
        colors.disabledColor = Color.white;
        colors.colorMultiplier = 1f;
        cardButton.colors = colors;

        cardButton.onClick.RemoveAllListeners();
        cardButton.onClick.AddListener(HandleCardClick);
    }

    private void ApplyEmptyCardState()
    {
        if (currentValueText != null)
        {
            currentValueText.text = "Cur: -";
        }

        SetInterpretation("No Data", "           ");
    }

    private List<scr_TransformGraphDataProvider.GraphPoint> GetVisiblePoints(
        List<scr_TransformGraphDataProvider.GraphPoint> source)
    {
        if (source == null || source.Count == 0)
        {
            return new List<scr_TransformGraphDataProvider.GraphPoint>();
        }

        if (source.Count <= visiblePointCount)
        {
            return new List<scr_TransformGraphDataProvider.GraphPoint>(source);
        }

        return source.GetRange(source.Count - visiblePointCount, visiblePointCount);
    }

    private void GetMinMax(
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

        if (Mathf.Approximately(minValue, maxValue))
        {
            minValue -= 0.5f;
            maxValue += 0.5f;
        }
    }

    private void CreateLine(Vector2 startPoint, Vector2 endPoint)
    {
        GameObject lineObject = new GameObject("Line", typeof(Image));
        lineObject.transform.SetParent(lineContainer, false);

        Image image = lineObject.GetComponent<Image>();
        image.color = lineColor;

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

        spawnedObjects.Add(lineObject);
    }

    private void CreatePoint(Vector2 position)
    {
        GameObject pointObject = new GameObject("Point", typeof(Image));
        pointObject.transform.SetParent(pointContainer, false);

        Image image = pointObject.GetComponent<Image>();
        image.color = lineColor;

        RectTransform rectTransform = pointObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = pointSize;
        rectTransform.anchoredPosition = position;

        spawnedObjects.Add(pointObject);
    }

    private void ClearGraph()
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(spawnedObjects[i]);
            }
        }

        spawnedObjects.Clear();
    }

    private void HandleCardClick()
    {
        if (!string.IsNullOrWhiteSpace(objectName))
        {
            clickCallback?.Invoke(objectName);
        }
    }

    private void RefreshCardVisual()
    {
        if (backgroundImage == null)
        {
            return;
        }

        backgroundImage.color = isSelected ? selectedColor : normalColor;
    }
}
