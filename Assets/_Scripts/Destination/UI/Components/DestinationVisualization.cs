using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Destination.UI
{
    public class DestinationVisualization : MonoBehaviour
    {
        [SerializeField]
        private Image destinationImage;

        [SerializeField]
        private TextMeshProUGUI destinationNameText;

        public void Init(IDestinationInfo destinationInfo)
        {
            destinationNameText.text = destinationInfo.GetName();
            destinationImage.sprite = destinationInfo.GetSprite();
        }
    }
}
