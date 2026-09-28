using System;
using QuizGame.Item.Interfaces;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Gameplay
{
    public class LuckyDrawResultUI : BaseUI
    {
        public event Action onAcceptButtonClicked;

        [SerializeField]
        private TextMeshProUGUI itemTierText;

        [SerializeField]
        private TextMeshProUGUI itemNameText;

        [SerializeField]
        private ImageWithTextVisualization itemVisualization;

        [SerializeField]
        private Button acceptButton;

        private void Start()
        {
            acceptButton.onClick.AddListener(() => { acceptButton.interactable = false; onAcceptButtonClicked?.Invoke(); });
        }

        public void Setup(IQuantifiableItem item)
        {
            var background = transform.Find("BG");
            if (background != null) background.GetComponent<Image>().color = new Color(.18f, .16f, .22f, 1);
            SetRect(itemTierText.rectTransform, new Vector2(.1f,.73f), new Vector2(.9f,.84f));
            SetRect(itemNameText.rectTransform, new Vector2(.1f,.30f), new Vector2(.9f,.41f));
            var visualRect = itemVisualization.GetComponent<RectTransform>();
            visualRect.anchorMin = visualRect.anchorMax = new Vector2(.5f,.56f);
            visualRect.anchoredPosition = Vector2.zero;
            itemTierText.transform.SetAsLastSibling(); itemNameText.transform.SetAsLastSibling();
            itemTierText.enableAutoSizing = itemNameText.enableAutoSizing = true;
            itemTierText.fontSizeMin = itemNameText.fontSizeMin = 24;
            itemTierText.fontSizeMax = itemNameText.fontSizeMax = 64;
            itemTierText.text = item.GetItemType() == ItemType.Material ? "Material" : item.GetItemTier().ToString() + " Item"; //TODO: Replace with some localization of item tier
            itemNameText.text = item.GetName();
            itemVisualization.Setup(item.GetSprite(), $"x{item.GetQuantity()}");
        }

        private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
