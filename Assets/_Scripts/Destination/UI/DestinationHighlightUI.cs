using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Destination.UI
{
    public class DestinationHighlightUI : BaseUI
    {
        [SerializeField]
        private Image destinationImage;

        [SerializeField]
        private TextMeshProUGUI destinationNameText;

        [SerializeField]
        private TextMeshProUGUI informationText;

        public void Init(IDestinationInfo destinationInfo)
        {
            destinationImage.sprite = destinationInfo.GetSprite();
            destinationNameText.text = destinationInfo.GetName();
            informationText.text = destinationInfo.GetDescription();
        }
    }
}
