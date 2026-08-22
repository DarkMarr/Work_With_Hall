using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace QuizGame.UI.Graph
{
    public class RadarGraph : Graph
    {
        [SerializeField]
        private Sprite valuePointSprite;
        [SerializeField]
        private float valuePointSize = 8;
        [SerializeField]
        private Color valuePointColor = Color.white;

        [SerializeField]
        private Material radarMeshMaterial;
        [SerializeField] 
        private TextMeshProUGUI radarGraphStatLabel;

        [SerializeField] 
        private float radarTextSpacing;

        public class StatData
        {
            public string Name;
            public float Value;
            public string QuestionType;
        }

        public void Setup(List<StatData> statsList, float statMaxValue)
        {
            Clear(GraphContainer);

            var radarMesh = GraphContainer.GetComponent<CanvasRenderer>();

            float radarChartSize = GraphContainer.rect.width;

            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            vertices.Add(Vector3.zero); // Center vertex

            for (int i = 0; i < statsList.Count; i++)
            {
                // Normalize the stat value relative to statMaxValue and scale it to radarChartSize
                float normalizedValue = statsList[i].Value / statMaxValue;

                // Calculate the angle for the current stat, starting at the top and moving clockwise
                float angle = Mathf.PI / 2 - (i * Mathf.PI * 2 / statsList.Count);

                // Calculate the vertex position
                Vector3 vertex = new Vector3(
                    Mathf.Cos(angle) * (radarChartSize / 2),
                    Mathf.Sin(angle) * (radarChartSize / 2),
                    0
                );

                var pointConnection = CreatePointConnection(
                    posA: Vector2.zero,
                    posB: vertex,
                    lineWidth: 4f,
                    color: Color.white,
                    parent: GraphContainer
                );

                pointConnection.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                pointConnection.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                // Create a text label for the stat
                var textLabel = Instantiate(radarGraphStatLabel, GraphContainer);
                textLabel.text = statsList[i].Name;
                textLabel.rectTransform.anchoredPosition = vertex + (vertex.normalized * radarTextSpacing);

                vertex *= normalizedValue;

                // Add vertex to the list
                vertices.Add(vertex);
                int currentIndex = vertices.Count - 1;
                if (currentIndex > 1)
                {
                    triangles.Add(0);
                    triangles.Add(currentIndex - 1);
                    triangles.Add(currentIndex);
                }

                // Create a point for the radar graph
                var point = CreatePoint(
                    anchoredPos: vertex,
                    size: valuePointSize,
                    sprite: valuePointSprite,
                    color: valuePointColor,
                    parent: GraphContainer
                );
                point.anchorMin = new Vector2(0.5f, 0.5f);
                point.anchorMax = new Vector2(0.5f, 0.5f);
            }

            // Close the mesh if there are enough vertices
            if (vertices.Count > 2)
            {
                triangles.Add(0);
                triangles.Add(vertices.Count - 1);
                triangles.Add(1);
            }

            // Assign data to mesh
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            // Set the UVs and normals
            radarMesh.SetMesh(mesh);
            radarMesh.SetMaterial(radarMeshMaterial, null);
        }
    }
}