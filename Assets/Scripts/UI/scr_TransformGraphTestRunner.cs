using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Graph Renderer 임시 테스트용
/// </summary>
public class scr_TransformGraphTestRunner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private scr_TransformGraphDataProvider graphDataProvider;
    [SerializeField] private scr_TransformGraphRenderer graphRenderer;

    [Header("Test Selection")]
    [SerializeField] private string targetObjectName = "Box_01";
    [SerializeField]
    private scr_TransformGraphDataProvider.TransformCategory targetCategory =
        scr_TransformGraphDataProvider.TransformCategory.Position;
    [SerializeField]
    private scr_TransformGraphDataProvider.AxisType targetAxis =
        scr_TransformGraphDataProvider.AxisType.X;

    [Header("Run Option")]
    [SerializeField] private bool runOnStart = true;

    private void Start()
    {
        if (runOnStart)
        {
            RunGraphTest();
        }
    }

    [ContextMenu("Run Graph Test")]
    public void RunGraphTest()
    {
        if (graphDataProvider == null || graphRenderer == null)
        {
            Debug.LogWarning("[GraphTestRunner] Missing reference.");
            return;
        }

        List<scr_TransformGraphDataProvider.GraphPoint> graphPoints =
            graphDataProvider.GetGraphPoints(targetObjectName, targetCategory, targetAxis);

        if (graphPoints == null || graphPoints.Count == 0)
        {
            Debug.LogWarning($"[GraphTestRunner] No graph data found for {targetObjectName} / {targetCategory} / {targetAxis}");
            return;
        }

        graphRenderer.RenderGraph(graphPoints);
        Debug.Log($"[GraphTestRunner] Graph test complete: {targetObjectName} / {targetCategory} / {targetAxis} / Count = {graphPoints.Count}");
    }
}
