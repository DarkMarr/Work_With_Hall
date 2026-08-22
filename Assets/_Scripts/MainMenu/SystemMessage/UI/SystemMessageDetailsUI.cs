using System;
using QuizGame.Item.Interfaces;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MainMenu.UI
{
    public class SystemMessageDetailsUI : BaseUI
    {
        private event Action OnAcceptItemButtonClicked;

        [SerializeField]
        private Button acceptItemButton;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private TextMeshProUGUI messageHeaderText;

        [SerializeField]
        private TextMeshProUGUI messageBodyText;

        [SerializeField]
        private Transform itemGivingContainer;

        [SerializeField]
        private ImageWithTextVisualization givingItemVisualizationPrefab;

        private void Start()
        {
            acceptItemButton.onClick.AddListener(() =>
            {
                OnAcceptItemButtonClicked?.Invoke();
                acceptItemButton.interactable = false;
            });
            closeButton.onClick.AddListener(Close);
        }

        public void SetOnAcceptItemButtonClickedEvent(Action action)
        {
            OnAcceptItemButtonClicked = action;
        }

        public void Init(IQuantifiableItem[] givingItems, string headerMessage, string bodyMessage, bool canAcceptItem = false)
        {
            messageHeaderText.text = headerMessage;
            messageBodyText.text = bodyMessage;
            acceptItemButton.interactable = canAcceptItem && givingItems != null && givingItems.Length >= 0;

            if (givingItems == null || givingItems.Length <= 0) return;

            foreach (var item in givingItems)
            {
                var itemVisualization = Instantiate(givingItemVisualizationPrefab, itemGivingContainer);
                itemVisualization.Setup(item.GetSprite(), $"x{item.GetQuantity()}");
            }
        }
    }
}
