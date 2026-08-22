using System;
using QuizGame.Interfaces;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Item.UI
{
    public class ItemSelectionUI : BaseItemSelectionUI<IHasSprite>
    {
        public Action OnCloseButtonClicked;

        [SerializeField]
        private Button selectItemButton;

        [SerializeField]
        private TextMeshProUGUI selectItemButtonText;

        [SerializeField]
        private TextMeshProUGUI itemTypeText;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private UIHighlighter highlighter;

        public void Init(int defaultSelectingItemIndex, string selectionTitle, IHasSprite[] itemSprites, Action onSelectButtonClicked)
        {
            Setup(defaultSelectingItemIndex, itemSprites);
            itemTypeText.text = selectionTitle;
            selectItemButton.onClick.AddListener(() => onSelectButtonClicked?.Invoke());
            closeButton.onClick.AddListener(() => OnCloseButtonClicked?.Invoke());
            closeButton.onClick.AddListener(Close);

            pageCategorySelectionButtons.OnIDChange += currentPage =>
            {
                var isSelectingItemInThePage = SelectingItemInPageIndex == currentPage && SelectingItemIndex > -1;
                highlighter.SetActive(isSelectingItemInThePage);
                SetSelectionButtonInteractable(isSelectingItemInThePage);
            };
        }

        public override void SetNullCurrentItemVisualization()
        {
            base.SetNullCurrentItemVisualization();
            SetSelectionButtonInteractable(false);
            highlighter.Hide();
        }

        public override void SetSelectingItemVisualization(SelectItemButton button, IHasSprite itemInfo)
        {
            base.SetSelectingItemVisualization(button, itemInfo);
            SetSelectionButtonInteractable(true);
            var buttonRect = button.GetComponent<RectTransform>();
            highlighter.SetTarget(buttonRect);
            highlighter.Show();
        }

        public void SetSelectionButtonInteractable(bool isInteractable)
        {
            selectItemButton.interactable = isInteractable;
        }

        public void SetButtonText(string message)
        {
            selectItemButtonText.text = message;
        }
    }
}
