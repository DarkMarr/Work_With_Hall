using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using QuizGame.Item.Interfaces;
using QuizGame.UI;
using QuizGame.Interfaces;

namespace QuizGame.Item.UI
{
    public class ItemWithInfoSelectionUI : BaseItemSelectionUI<IItem>
    {
        public event Action OnOkayButtonClicked;
        public event Action OnDeselectButtonClicked;

        [SerializeField]
        private UIHighlighter highlighter;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private Button okayButton;

        [SerializeField]
        private Button deselectItemButton;

        [Header("Staging UI")]
        [SerializeField]
        private GameObject itemInformationVisualization;

        [SerializeField]
        private GameObject NoItemVisualization;

        [Header("Current Item")]
        [SerializeField]
        private Image itemImage;

        [SerializeField]
        private TextMeshProUGUI itemNameText;

        [SerializeField]
        private TextMeshProUGUI itemSubDescriptionText;

        [SerializeField]
        private TextMeshProUGUI itemDescriptionText;

        protected virtual void Start()
        {
            okayButton?.onClick.AddListener(() => OnOkayButtonClicked?.Invoke());
            deselectItemButton?.onClick.AddListener(() => OnDeselectButtonClicked?.Invoke());
            pageCategorySelectionButtons.OnIDChange += currentPage =>
            {
                highlighter.SetActive(SelectingItemInPageIndex == currentPage && SelectingItemIndex > -1);
            };
            closeButton?.onClick.AddListener(Close);
        }

        public override void SetNullCurrentItemVisualization()
        {
            itemImage.sprite = null;
            itemNameText.text = "";
            itemSubDescriptionText.text = "";
            itemDescriptionText.text = "";

            highlighter.Hide();
            SetDeselectItemButtonActive(false);
            SetItemInfoUIActive(false);
        }

        public override void SetSelectingItemVisualization(SelectItemButton button, IItem itemInfo)
        {
            itemImage.sprite = itemInfo.GetSprite();
            itemNameText.text = itemInfo.GetName();

            if (itemInfo is IHasDescription descriptionItem)
            {
                itemDescriptionText.text = descriptionItem.GetDescription();

                if (itemInfo is IHasQuantity quantityItem)
                {
                    itemSubDescriptionText.text = string.Format(descriptionItem.GetSubDescription(), quantityItem.GetQuantity());
                }
                else
                {
                    itemSubDescriptionText.text = descriptionItem.GetSubDescription();
                }
            }

            var clickedButtonRect = button.GetComponent<RectTransform>();
            highlighter.Show();
            highlighter.SetTarget(clickedButtonRect);
            SetDeselectItemButtonActive(true);
            SetItemInfoUIActive(true);
        }

        private void SetDeselectItemButtonActive(bool isActive)
        {
            deselectItemButton?.gameObject.SetActive(isActive);
        }

        private void SetItemInfoUIActive(bool isActive)
        {
            itemInformationVisualization.SetActive(isActive);
            NoItemVisualization.SetActive(!isActive);
        }
    }
}
