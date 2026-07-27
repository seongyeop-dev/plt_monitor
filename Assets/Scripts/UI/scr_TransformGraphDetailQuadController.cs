using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Detail 4분면(X / Y / Z / All) 전용 컨트롤러
/// - 렌더만 담당
/// - 데이터 조회는 UIController가 담당하고, 준비된 seriesMap을 전달받음
/// - Detail 모드에서 Y축 범위와 Y라벨 배치를 고정해서 패널 간 비교 가독성 유지
/// </summary>
public class scr_TransformGraphDetailQuadController : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private scr_TransformGraphRenderer xRenderer;
    [SerializeField] private scr_TransformGraphRenderer yRenderer;
    [SerializeField] private scr_TransformGraphRenderer zRenderer;
    [SerializeField] private scr_TransformGraphRenderer allRenderer;

    [Header("Titles")]
    [SerializeField] private Text xTitleText;
    [SerializeField] private Text yTitleText;
    [SerializeField] private Text zTitleText;
    [SerializeField] private Text allTitleText;

    [Header("Legends")]
    [SerializeField] private Text xLegendText;
    [SerializeField] private Text yLegendText;
    [SerializeField] private Text zLegendText;
    [SerializeField] private Text allLegendText;

    private readonly Color xColor = new Color(1f, 0.35f, 0.35f, 1f);
    private readonly Color yColor = new Color(0.35f, 1f, 0.55f, 1f);
    private readonly Color zColor = new Color(0.20f, 0.85f, 1f, 1f);

    public void RenderDetail(
        string objectName,
        scr_TransformGraphDataProvider.TransformCategory category,
        Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>> seriesMap)
    {
        if (string.IsNullOrWhiteSpace(objectName) || seriesMap == null)
        {
            ClearAll();
            return;
        }

        List<scr_TransformGraphDataProvider.GraphPoint> xPoints = GetSeries(seriesMap, "X");
        List<scr_TransformGraphDataProvider.GraphPoint> yPoints = GetSeries(seriesMap, "Y");
        List<scr_TransformGraphDataProvider.GraphPoint> zPoints = GetSeries(seriesMap, "Z");

        ApplyDetailAxisSettings(category);

        xRenderer?.RenderSingle(xPoints, xColor);
        yRenderer?.RenderSingle(yPoints, yColor);
        zRenderer?.RenderSingle(zPoints, zColor);

        if (allRenderer != null)
        {
            allRenderer.RenderMultiSeries(seriesMap);
        }

        UpdateTitles(objectName, category);
        UpdateLegends(category);
    }

    public void ClearAll()
    {
        xRenderer?.ClearGraph();
        yRenderer?.ClearGraph();
        zRenderer?.ClearGraph();
        allRenderer?.ClearGraph();
    }

    private void ApplyDetailAxisSettings(scr_TransformGraphDataProvider.TransformCategory category)
    {
        ApplyCommonLabelLayout(xRenderer);
        ApplyCommonLabelLayout(yRenderer);
        ApplyCommonLabelLayout(zRenderer);
        ApplyCommonLabelLayout(allRenderer);

        switch (category)
        {
            case scr_TransformGraphDataProvider.TransformCategory.Position:

                xRenderer?.SetFixedYRange(true, -2f, 2f);
                yRenderer?.SetFixedYRange(true, 0f, 2f);
                zRenderer?.SetFixedYRange(true, -6f, -2f);
                allRenderer?.SetFixedYRange(true, -6f, 2f);
                break;

            case scr_TransformGraphDataProvider.TransformCategory.Rotation:

                xRenderer?.SetFixedYRange(true, -30f, 30f);
                yRenderer?.SetFixedYRange(true, 0f, 360f);
                zRenderer?.SetFixedYRange(true, -30f, 30f);
                allRenderer?.SetFixedYRange(true, -30f, 360f);
                break;

            case scr_TransformGraphDataProvider.TransformCategory.Scale:

                xRenderer?.SetFixedYRange(true, 0f, 1f);
                yRenderer?.SetFixedYRange(true, 0f, 1f);
                zRenderer?.SetFixedYRange(true, 0f, 1f);
                allRenderer?.SetFixedYRange(true, 0f, 1f);
                break;

            default:
                xRenderer?.SetFixedYRange(false, 0f, 0f);
                yRenderer?.SetFixedYRange(false, 0f, 0f);
                zRenderer?.SetFixedYRange(false, 0f, 0f);
                allRenderer?.SetFixedYRange(false, 0f, 0f);
                break;
        }
    }
    private void ApplyCommonLabelLayout(scr_TransformGraphRenderer renderer)
    {
        if (renderer == null)
        {
            return;
        }

        renderer.SetYLabelLayout(
            newLeftPadding: 84f,
            newOutsideOffset: 12f,
            newLabelSize: new Vector2(68f, 20f));

        renderer.SetYLabelRule(
            useEvenLabels: false,
            newLabelCount: 4);
    }

    private void UpdateTitles(string objectName, scr_TransformGraphDataProvider.TransformCategory category)
    {
        string categoryName = category.ToString();

        if (xTitleText != null) xTitleText.text = $"{objectName} - X Graph ({categoryName})";
        if (yTitleText != null) yTitleText.text = $"{objectName} - Y Graph ({categoryName})";
        if (zTitleText != null) zTitleText.text = $"{objectName} - Z Graph ({categoryName})";
        if (allTitleText != null) allTitleText.text = $"{objectName} - All Graph ({categoryName})";
    }

    private void UpdateLegends(scr_TransformGraphDataProvider.TransformCategory category)
    {
        string xLegend = "X";
        string yLegend = "Y";
        string zLegend = "Z";

        switch (category)
        {
            case scr_TransformGraphDataProvider.TransformCategory.Position:
                xLegend = "X - 좌우 편차";
                yLegend = "Y - 상하 변화";
                zLegend = "Z - 전방 이송";
                break;

            case scr_TransformGraphDataProvider.TransformCategory.Rotation:
                xLegend = "X - 앞뒤 기울기";
                yLegend = "Y - 좌우 기울기";
                zLegend = "Z - 방향 정렬";
                break;

            case scr_TransformGraphDataProvider.TransformCategory.Scale:
                xLegend = "X - 가로 크기";
                yLegend = "Y - 세로 크기";
                zLegend = "Z - 깊이 크기";
                break;
        }

        SetLegendText(xLegendText, $"<color=#FF5A5A><b>{xLegend}</b></color>");
        SetLegendText(yLegendText, $"<color=#59FF8D><b>{yLegend}</b></color>");
        SetLegendText(zLegendText, $"<color=#33D7FF><b>{zLegend}</b></color>");
        SetLegendText(
            allLegendText,
            "<color=#FF5A5A><b>X</b></color> / " +
            "<color=#59FF8D><b>Y</b></color> / " +
            "<color=#33D7FF><b>Z</b></color> Compare");
    }

    private void SetLegendText(Text targetText, string value)
    {
        if (targetText == null)
        {
            return;
        }

        targetText.supportRichText = true;
        targetText.text = value;
    }

    private List<scr_TransformGraphDataProvider.GraphPoint> GetSeries(
        Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>> seriesMap,
        string key)
    {
        if (seriesMap.TryGetValue(key, out List<scr_TransformGraphDataProvider.GraphPoint> points))
        {
            return points;
        }

        return new List<scr_TransformGraphDataProvider.GraphPoint>();
    }
}