using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.UI
{
    public class ImageWithTextVisualization : MonoBehaviour
    {
        [SerializeField]
        private Image image;

        [SerializeField]
        private TextMeshProUGUI text;

        public void Setup(Sprite sprite, string message)
        {
            image.sprite = sprite;
            text.text = message;
        }
    }
}
