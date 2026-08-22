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
            acceptButton.onClick.AddListener(() => onAcceptButtonClicked?.Invoke());
        }

        public void Setup(IQuantifiableItem item)
        {
            itemTierText.text = item.GetItemTier().ToString() + " Item"; //TODO: Replace with some localization of item tier
            itemNameText.text = item.GetName();
            itemVisualization.Setup(item.GetSprite(), $"x{item.GetQuantity()}");
        }
    }
}
