using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace QuizGame.UI.Graph
{
    public class LineGraph : Graph
    {
        [SerializeField] private TextMeshProUGUI lineGraphLabel;

        [Header("Graph Settings")]
        [SerializeField] private Sprite statPointSprite;
        [SerializeField] private float statPointSize = 10f;
        [SerializeField] private float statLineWidth = 5f;
        [SerializeField] private float graphLineWidth = 5f;

        [Header("Graph Colors")]
        [SerializeField] private Color statPointColor = Color.white;

        [SerializeField] private Color statLineColor = Color.white;
        [SerializeField] private Color graphLineColor = Color.white;

        public void Setup(List<string> labelList, Vector2Int gridSize, List<int> valueList, bool isDrawHorizontalLine = false)
        {
            Clear(GraphContainer);

            var width = GraphContainer.rect.width;
            var unitWidth = width / (gridSize.x);

            for (int i = 0; i < labelList.Count; i++)
            {
                var label = Instantiate(lineGraphLabel, GraphContainer);
                label.text = labelList[i];
                label.rectTransform.anchoredPosition = new Vector2(0, GetYPositionForRank(i, gridSize));

                label.alignment = TextAlignmentOptions.Right;

                if (isDrawHorizontalLine)
                    DrawGraphHorizontalLine(
                        pos: new Vector2(unitWidth, GetYPositionForRank(i, gridSize)),
                        lineSize: new Vector2(GraphContainer.rect.width - unitWidth, graphLineWidth), 
                        gridSize: gridSize, 
                        parent: GraphContainer
                    );
            }

            var lastPoint = new RectTransform();

            for (int i = 0; i < valueList.Count; i++)
            {
                float xPosition = (i + 1) * unitWidth; // Prevent to multiply with i(0) for padding
                float yPosition = GetYPositionForRank(valueList[i], gridSize);

                var point = CreatePoint(
                    anchoredPos: new Vector2(xPosition, yPosition),
                    size: statPointSize,
                    sprite: statPointSprite,
                    color: statPointColor,
                    parent: GraphContainer
                );

                if (lastPoint != default)
                {
                    var connector = CreatePointConnection(
                        lastPoint.anchoredPosition, 
                        point.anchoredPosition, 
                        lineWidth: statLineWidth, 
                        color: statLineColor, 
                        parent: GraphContainer
                    );

                    // Show the prev point on top
                    lastPoint.SetAsLastSibling();
                }
                lastPoint = point;
            }
            // Show the last point on top
            lastPoint.SetAsLastSibling();
        }

        private void DrawGraphHorizontalLine(Vector2 pos, Vector2 lineSize, Vector2Int gridSize, Transform parent)
        {
            for (int i = 0; i < gridSize.y + 1; i++)
            {
                var line = CreateImage("HorizontalLine", parent);
                line.color = graphLineColor;
                line.rectTransform.sizeDelta = lineSize;
                var yPosition = GetYPositionForRank(i, gridSize);
                line.rectTransform.anchoredPosition = pos;
                line.rectTransform.pivot = new Vector2(0f, 0.5f);
            }
        }

        private float GetYPositionForRank(int rank, Vector2Int gridSize)
        {
            float graphHeight = GraphContainer.rect.height;
            return (rank / (float)gridSize.y) * graphHeight;
        }
    }
}