using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.UI
{
    public class ConfirmPopupUI : BaseUI
    {
        [SerializeField]
        private Button confirmButton;

        [SerializeField]
        private Button cancelButton;

        [SerializeField]
        private TextMeshProUGUI TitleText;

        [SerializeField]
        private TextMeshProUGUI descriptionText;

        private event Action onConfirmButtonClicked;
        private event Action onCancelButtonClicked;

        protected override void Awake()
        {
            base.Awake();
            confirmButton.onClick.AddListener(() => onConfirmButtonClicked?.Invoke());
            cancelButton.onClick.AddListener(() => onCancelButtonClicked?.Invoke());
        }

        public void Setup(string title, string description, Action onConfirmButtonClicked, Action onCancelButtonClicked)
        {
            TitleText.text = title;
            descriptionText.text = description;
            this.onConfirmButtonClicked = onConfirmButtonClicked;
            this.onCancelButtonClicked = onCancelButtonClicked;
            Show();
        }
    }
}
