using QuizGame.MyRoom.Decoration;
using QuizGame.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Fuse.UI
{
    public class FusingPreviewUI : BaseUI
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

        public void Init(IDecorationItem decorationItem)
        {
            itemIcon.sprite = decorationItem.GetSprite();
        }
    }
}