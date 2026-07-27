using System;
using System.Collections.Generic;
using UnityEngine;

public class scr_TransformGraphOverviewController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private scr_TransformGraphDataProvider graphDataProvider;
    [SerializeField] private RectTransform cardGrid;
    [SerializeField] private scr_TransformOverviewGraphCard cardPrefab;

    private readonly List<scr_TransformOverviewGraphCard> spawnedCards = new List<scr_TransformOverviewGraphCard>();
    private readonly Dictionary<string, scr_TransformOverviewGraphCard> cardMap = new Dictionary<string, scr_TransformOverviewGraphCard>();

    public Action<string> OnOverviewCardClicked;

    public void BuildCards()
    {
        ClearCards();

        if (graphDataProvider == null || cardGrid == null || cardPrefab == null)
        {
            Debug.LogWarning("[OverviewController] Missing reference.");
            return;
        }

        List<string> objectNames = graphDataProvider.GetObjectNameList();

        for (int i = 0; i < objectNames.Count; i++)
        {
            string objectName = objectNames[i];
            scr_TransformOverviewGraphCard card = Instantiate(cardPrefab, cardGrid);
            card.gameObject.name = $"Card_{objectName}";
            card.Setup(objectName, HandleCardClicked);

            spawnedCards.Add(card);
            cardMap[objectName] = card;
        }
    }

    public void RefreshCards(
        scr_TransformGraphDataProvider.TransformCategory category,
        scr_TransformGraphDataProvider.AxisType axis)
    {
        if (graphDataProvider == null)
        {
            return;
        }

        if (spawnedCards.Count == 0)
        {
            BuildCards();
        }

        for (int i = 0; i < spawnedCards.Count; i++)
        {
            scr_TransformOverviewGraphCard card = spawnedCards[i];
            if (card == null)
            {
                continue;
            }

            string objectName = card.GetObjectName();
            List<scr_TransformGraphDataProvider.GraphPoint> points =
                graphDataProvider.GetGraphPoints(objectName, category, axis);

            card.SetCardTitle(objectName);
            card.RenderGraph(points);

            scr_TransformGraphMeaningAnalyzer.AxisAnalysisResult analysis =
                scr_TransformGraphMeaningAnalyzer.AnalyzeAxis(points, category, axis);

            card.SetInterpretation(analysis.shortState, analysis.meaning);
        }
    }

    public void SetSelectedCard(string selectedObjectName)
    {
        for (int i = 0; i < spawnedCards.Count; i++)
        {
            scr_TransformOverviewGraphCard card = spawnedCards[i];
            if (card == null)
            {
                continue;
            }

            card.SetSelected(card.GetObjectName() == selectedObjectName);
        }
    }

    private void HandleCardClicked(string objectName)
    {
        OnOverviewCardClicked?.Invoke(objectName);
    }

    private void ClearCards()
    {
        for (int i = 0; i < spawnedCards.Count; i++)
        {
            if (spawnedCards[i] != null)
            {
                Destroy(spawnedCards[i].gameObject);
            }
        }

        spawnedCards.Clear();
        cardMap.Clear();
    }
}
