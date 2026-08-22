using QuizGame.Interfaces;
using QuizGame.Item.Interfaces;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MyDiary.UI
{
    public class ItemPopupInfoUI : BaseUI
    {
        [SerializeField]
        private Image itemImage;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private TextMeshProUGUI itemNameText;

        [SerializeField]
        private TextMeshProUGUI itemTypeText;

        [SerializeField]
        private TextMeshProUGUI descriptionText;

        private void Start()
        {
            closeButton.onClick.AddListener(Close);
        }

        public void Setup(IItem item)
        {
            SetupDescriptionItem(item);

            itemNameText.text = item.GetName();
            itemTypeText.text = item.GetItemType().ToString(); //TODO: Replace with localization
            itemImage.sprite = item.GetSprite();
        }

        private void SetupDescriptionItem(IItem item)
        {
            if (item is IHasDescription descriptionItem)
            {
                descriptionText.text = descriptionItem.GetDescription();
            }
            else
            {
                descriptionText.text = "";
            }
        }
    }
}