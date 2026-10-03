using QuizGame.Item.Interfaces;
using QuizGame.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Craft.UI
{
    public class CraftingPreviewUI : BaseUI
    {
        public event Action OnCloseButtonClicked;

        [SerializeField]
        private Image itemIcon;

        [SerializeField]
        private Button closeButton;

        private void Start()
        {
            closeButton.onClick.AddListener(() => OnCloseButtonClicked?.Invoke());
        }

        public void Init(ICraftableItem craftItem)
        {
            itemIcon.sprite = craftItem.GetSprite();
        }
    }
}