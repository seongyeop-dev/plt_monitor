using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Transform Graph 해석 전용 
///
/// 기준:
/// - 현재 프로젝트에서는 Position Z를 컨베이어 전방 이송축으로 가정
/// - Position X = 좌우 편차
/// - Position Y = 상하 흔들림
/// - Rotation X/Y = 기울어짐
/// - Rotation Z = 방향 정렬
/// - Scale = 크기 유지/변형 여부
/// </summary>
public static class scr_TransformGraphMeaningAnalyzer
{
    public class AxisAnalysisResult
    {
        public string shortState = "-";
        public string meaning = "-";
        public string risk = "-";
        public string insight = "-";
        public float delta = 0f;
        public float range = 0f;
        public bool isStable = true;
    }

    public class DetailAnalysisResult
    {
        public string conveyorState = "-";
        public string axisMeaning = "-";
        public string risk = "-";
        public string insight = "-";
    }

    private const int DefaultWindowCount = 30;

    public static AxisAnalysisResult AnalyzeAxis(
        List<scr_TransformGraphDataProvider.GraphPoint> points,
        scr_TransformGraphDataProvider.TransformCategory category,
        scr_TransformGraphDataProvider.AxisType axis)
    {
        AxisAnalysisResult result = new AxisAnalysisResult();

        List<scr_TransformGraphDataProvider.GraphPoint> window = GetRecentWindow(points, DefaultWindowCount);

        if (window == null || window.Count < 2)
        {
            result.shortState = "No Data";
            result.meaning = "데이터 부족";
            result.risk = "확인 필요";
            result.insight = "최근 프레임 수가 부족하여 상태 해석이 어렵습니다.";
            result.isStable = false;
            return result;
        }

        float minValue = window[0].value;
        float maxValue = window[0].value;

        for (int i = 1; i < window.Count; i++)
        {
            float value = window[i].value;

            if (value < minValue)
            {
                minValue = value;
            }

            if (value > maxValue)
            {
                maxValue = value;
            }
        }

        float startValue = window[0].value;
        float endValue = window[window.Count - 1].value;
        float delta = endValue - startValue;
        float range = maxValue - minValue;

        result.delta = delta;
        result.range = range;

        switch (category)
        {
            case scr_TransformGraphDataProvider.TransformCategory.Position:
                AnalyzePosition(result, axis, delta, range);
                break;

            case scr_TransformGraphDataProvider.TransformCategory.Rotation:
                AnalyzeRotation(result, axis, delta, range);
                break;

            case scr_TransformGraphDataProvider.TransformCategory.Scale:
                AnalyzeScale(result, axis, delta, range);
                break;
        }

        return result;
    }

    public static DetailAnalysisResult AnalyzeDetail(
        Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>> seriesMap,
        scr_TransformGraphDataProvider.TransformCategory category)
    {
        DetailAnalysisResult result = new DetailAnalysisResult();

        List<scr_TransformGraphDataProvider.GraphPoint> xPoints = GetSeries(seriesMap, "X");
        List<scr_TransformGraphDataProvider.GraphPoint> yPoints = GetSeries(seriesMap, "Y");
        List<scr_TransformGraphDataProvider.GraphPoint> zPoints = GetSeries(seriesMap, "Z");

        AxisAnalysisResult x = AnalyzeAxis(
            xPoints,
            category,
            scr_TransformGraphDataProvider.AxisType.X);

        AxisAnalysisResult y = AnalyzeAxis(
            yPoints,
            category,
            scr_TransformGraphDataProvider.AxisType.Y);

        AxisAnalysisResult z = AnalyzeAxis(
            zPoints,
            category,
            scr_TransformGraphDataProvider.AxisType.Z);

        switch (category)
        {
            case scr_TransformGraphDataProvider.TransformCategory.Position:
                BuildPositionDetail(result, x, y, z);
                break;

            case scr_TransformGraphDataProvider.TransformCategory.Rotation:
                BuildRotationDetail(result, x, y, z);
                break;

            case scr_TransformGraphDataProvider.TransformCategory.Scale:
                BuildScaleDetail(result, x, y, z);
                break;
        }

        return result;
    }

    private static void AnalyzePosition(
        AxisAnalysisResult result,
        scr_TransformGraphDataProvider.AxisType axis,
        float delta,
        float range)
    {
        float stableDelta = 0.01f;
        float stableRange = 0.02f;

        result.isStable =
            Mathf.Abs(delta) < stableDelta &&
            range < stableRange;

        if (axis == scr_TransformGraphDataProvider.AxisType.Z)
        {
            if (delta > 0.05f)
            {
                result.shortState = "Forward";
                result.meaning = "전방 이송";
                result.risk = "낮음";
                result.insight = "Z축 값이 증가하여 컨베이어 전방으로 이동 중입니다.";
            }
            else if (delta < -0.05f)
            {
                result.shortState = "Reverse";
                result.meaning = "역방향 이동";
                result.risk = "주의";
                result.insight = "Z축 값이 감소하여 역방향 이동이 감지됩니다.";
                result.isStable = false;
            }
            else if (result.isStable)
            {
                result.shortState = "Stop";
                result.meaning = "정지/대기";
                result.risk = "낮음";
                result.insight = "전방 이송축 변화가 거의 없어 정지 또는 대기 상태로 보입니다.";
            }
            else
            {
                result.shortState = "Unsteady";
                result.meaning = "이송 불안정";
                result.risk = "주의";
                result.insight = "전방 이송축 변화는 있으나 패턴이 불안정합니다.";
                result.isStable = false;
            }

            return;
        }

        if (axis == scr_TransformGraphDataProvider.AxisType.X)
        {
            if (result.isStable)
            {
                result.shortState = "Stable";
                result.meaning = "중심 유지";
                result.risk = "낮음";
                result.insight = "좌우 편차가 거의 없어 중심을 안정적으로 유지합니다.";
            }
            else if (range > 0.05f || Mathf.Abs(delta) > 0.03f)
            {
                result.shortState = "Drift";
                result.meaning = "좌우 편차";
                result.risk = "주의";
                result.insight = "좌우 방향 위치 변화가 커서 라인 중심 이탈 경향이 있습니다.";
                result.isStable = false;
            }
            else
            {
                result.shortState = "Minor Move";
                result.meaning = "미세 좌우 이동";
                result.risk = "낮음";
                result.insight = "좌우 방향에 작은 위치 변화가 있습니다.";
            }

            return;
        }

        if (result.isStable)
        {
            result.shortState = "Stable";
            result.meaning = "높이 안정";
            result.risk = "낮음";
            result.insight = "상하 높이 변화가 거의 없어 안정적인 상태입니다.";
        }
        else if (range > 0.05f || Mathf.Abs(delta) > 0.03f)
        {
            result.shortState = "Bounce";
            result.meaning = "상하 흔들림";
            result.risk = "주의";
            result.insight = "상하 높이 변동 폭이 커서 흔들림이 감지됩니다.";
            result.isStable = false;
        }
        else
        {
            result.shortState = "Minor Move";
            result.meaning = "미세 높이 변화";
            result.risk = "낮음";
            result.insight = "상하 축에 작은 변화가 있습니다.";
        }
    }

    private static void AnalyzeRotation(
        AxisAnalysisResult result,
        scr_TransformGraphDataProvider.AxisType axis,
        float delta,
        float range)
    {
        float stableDelta = 0.5f;
        float stableRange = 1.0f;

        result.isStable =
            Mathf.Abs(delta) < stableDelta &&
            range < stableRange;

        if (result.isStable)
        {
            result.shortState = "Stable";
            result.meaning = "회전 안정";
            result.risk = "낮음";
            result.insight = "회전 변화가 작아 정렬이 안정적입니다.";
            return;
        }

        if (axis == scr_TransformGraphDataProvider.AxisType.Z)
        {
            result.shortState = "Rotate";
            result.meaning = "방향 정렬 변화";
            result.risk = "주의";
            result.insight = "Z축 회전 변화가 커서 진행 방향 정렬 이상 가능성이 있습니다.";
            result.isStable = false;
        }
        else
        {
            result.shortState = "Tilt";
            result.meaning = "기울어짐";
            result.risk = "주의";
            result.insight = "X/Y 회전 변화가 커서 물체가 기울어지는 패턴이 감지됩니다.";
            result.isStable = false;
        }
    }

    private static void AnalyzeScale(
        AxisAnalysisResult result,
        scr_TransformGraphDataProvider.AxisType axis,
        float delta,
        float range)
    {
        float stableDelta = 0.005f;
        float stableRange = 0.01f;

        result.isStable =
            Mathf.Abs(delta) < stableDelta &&
            range < stableRange;

        if (result.isStable)
        {
            result.shortState = "Stable";
            result.meaning = "크기 유지";
            result.risk = "낮음";
            result.insight = "Scale 변화가 거의 없어 크기를 정상적으로 유지합니다.";
            return;
        }

        if (delta > 0.02f)
        {
            result.shortState = "Expand";
            result.meaning = axis + "축 확대";
            result.risk = "주의";
            result.insight = "Scale 값이 증가하여 크기 확대 또는 측정 기준 변화가 감지됩니다.";
        }
        else if (delta < -0.02f)
        {
            result.shortState = "Shrink";
            result.meaning = axis + "축 축소";
            result.risk = "주의";
            result.insight = "Scale 값이 감소하여 크기 축소 또는 측정 기준 변화가 감지됩니다.";
        }
        else
        {
            result.shortState = "Scale Alert";
            result.meaning = "크기 변동";
            result.risk = "주의";
            result.insight = "Scale 변화가 감지되어 정상 고정 상태가 아닙니다.";
        }

        result.isStable = false;
    }

    private static void BuildPositionDetail(
        DetailAnalysisResult result,
        AxisAnalysisResult x,
        AxisAnalysisResult y,
        AxisAnalysisResult z)
    {
        if (z.shortState == "Forward" && x.isStable && y.isStable)
        {
            result.conveyorState = "정상 이송 중";
            result.axisMeaning = "Z축 기준 전방 이송, X/Y 안정";
            result.risk = "낮음";
            result.insight = "전방 이동이 안정적으로 진행되고 있으며 좌우/상하 편차가 거의 없습니다.";
            return;
        }

        if (z.shortState == "Stop" && x.isStable && y.isStable)
        {
            result.conveyorState = "정지 또는 대기";
            result.axisMeaning = "이송축 변화 거의 없음";
            result.risk = "낮음";
            result.insight = "현재 물체가 정지하거나 다음 공정을 대기하는 상태로 보입니다.";
            return;
        }

        if (z.shortState == "Reverse")
        {
            result.conveyorState = "역방향 이동 감지";
            result.axisMeaning = "Z축 기준 후진";
            result.risk = "주의";
            result.insight = "전방 이송이 아닌 반대 방향 이동이 감지됩니다.";
            return;
        }

        if (!x.isStable && y.isStable)
        {
            result.conveyorState = "좌우 편차 발생";
            result.axisMeaning = "X축 변동 큼";
            result.risk = "주의";
            result.insight = "전방 이동 중 좌우 편차가 커서 중심 유지 확인이 필요합니다.";
            return;
        }

        if (!y.isStable)
        {
            result.conveyorState = "상하 흔들림 감지";
            result.axisMeaning = "Y축 변동 큼";
            result.risk = "주의";
            result.insight = "상하 진동 또는 높이 불안정이 감지됩니다.";
            return;
        }

        result.conveyorState = "이송 패턴 확인 필요";
        result.axisMeaning = "복합 위치 변화";
        result.risk = "주의";
        result.insight = "위치 변화가 감지되지만 전형적인 정상 이송 패턴과 다릅니다.";
    }

    private static void BuildRotationDetail(
        DetailAnalysisResult result,
        AxisAnalysisResult x,
        AxisAnalysisResult y,
        AxisAnalysisResult z)
    {
        if (x.isStable && y.isStable && z.isStable)
        {
            result.conveyorState = "회전 이상 없음";
            result.axisMeaning = "기울어짐/방향 변화 안정";
            result.risk = "낮음";
            result.insight = "회전 변화가 작아 정렬이 잘 유지되고 있습니다.";
            return;
        }

        if (!z.isStable)
        {
            result.conveyorState = "방향 정렬 이상";
            result.axisMeaning = "Rotation Z 변화";
            result.risk = "주의";
            result.insight = "진행 방향이 틀어질 가능성이 있어 정렬 상태 확인이 필요합니다.";
            return;
        }

        result.conveyorState = "기울어짐 감지";
        result.axisMeaning = "Rotation X/Y 변화";
        result.risk = "주의";
        result.insight = "물체가 앞뒤 또는 좌우로 기울어지는 패턴이 감지됩니다.";
    }

    private static void BuildScaleDetail(
        DetailAnalysisResult result,
        AxisAnalysisResult x,
        AxisAnalysisResult y,
        AxisAnalysisResult z)
    {
        if (x.isStable && y.isStable && z.isStable)
        {
            result.conveyorState = "크기 유지 정상";
            result.axisMeaning = "Scale 변화 거의 없음";
            result.risk = "낮음";
            result.insight = "형상/크기 값이 안정적으로 유지됩니다.";
            return;
        }

        result.conveyorState = "크기 변화 감지";
        result.axisMeaning = "Scale 축 변동 발생";
        result.risk = "주의";
        result.insight = "Scale 값 변동이 있어 형상 왜곡 또는 측정 기준 이상 가능성이 있습니다.";
    }

    private static List<scr_TransformGraphDataProvider.GraphPoint> GetRecentWindow(
        List<scr_TransformGraphDataProvider.GraphPoint> points,
        int count)
    {
        if (points == null || points.Count == 0)
        {
            return new List<scr_TransformGraphDataProvider.GraphPoint>();
        }

        if (points.Count <= count)
        {
            return new List<scr_TransformGraphDataProvider.GraphPoint>(points);
        }

        int startIndex = points.Count - count;
        return points.GetRange(startIndex, count);
    }

    private static List<scr_TransformGraphDataProvider.GraphPoint> GetSeries(
        Dictionary<string, List<scr_TransformGraphDataProvider.GraphPoint>> seriesMap,
        string key)
    {
        if (seriesMap == null)
        {
            return new List<scr_TransformGraphDataProvider.GraphPoint>();
        }

        if (seriesMap.TryGetValue(key, out List<scr_TransformGraphDataProvider.GraphPoint> points))
        {
            return points;
        }

        return new List<scr_TransformGraphDataProvider.GraphPoint>();
    }
}