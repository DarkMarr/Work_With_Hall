using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.UI.Graph
{
    public abstract class Graph : MonoBehaviour
    {
        [SerializeField] protected RectTransform GraphContainer;

        protected void Clear(Transform parent)
        {
            foreach (Transform child in parent)
            {
                Destroy(child.gameObject);
            }
        }

        protected Image CreateImage(string name, Transform parent)
        {
            var image = new GameObject(name, typeof(Image)).GetComponent<Image>();
            image.transform.SetParent(parent, false);
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.zero;
            return image;
        }

        protected RectTransform CreatePoint(Vector2 anchoredPos, float size, Sprite sprite, Color color, Transform parent)
        {
            var point = CreateImage("Point", parent);

            point.sprite = sprite;
            point.color = color;

            point.rectTransform.anchoredPosition = anchoredPos;
            point.rectTransform.sizeDelta = new Vector2(size, size);

            return point.rectTransform;
        }

        protected Image CreatePointConnection(Vector2 posA, Vector2 posB, float lineWidth, Color color, Transform parent)
        {
            var pointConnection = CreateImage("DotConnection", parent);
            pointConnection.color = color;
            pointConnection.rectTransform.sizeDelta = new Vector2(Vector2.Distance(posA, posB), lineWidth);
            pointConnection.rectTransform.anchoredPosition = (posA + posB) / 2;
            pointConnection.rectTransform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, posB - posA));

            return pointConnection;
        }
    }
}