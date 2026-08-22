using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.UI
{
    public class MessagePopupUI : BaseUI
    {
        [SerializeField]
        private Button messageButton;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private TextMeshProUGUI buttonText;

        [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField]
        private TextMeshProUGUI descriptionText;

        private event Action onMessageButtonClicked;
        private event Action onCloseButtonClicked;

        protected override void Awake()
        {
            base.Awake();
            messageButton.onClick.AddListener(() => onMessageButtonClicked?.Invoke());
            closeButton.onClick.AddListener(() => onCloseButtonClicked?.Invoke());
        }

        public void Setup(string title, string description, string buttonMessage, Action onMessageButtonClicked, Action onCloseButtonClicked = null)
        {
            titleText.text = title;
            descriptionText.text = description;
            buttonText.text = buttonMessage;
            this.onMessageButtonClicked = onMessageButtonClicked;
            if (onCloseButtonClicked == null)
            {
                SetCloseButtonActive(false);
            }
            else
            {
                SetCloseButtonActive(true);
                this.onCloseButtonClicked = onCloseButtonClicked;
            }
        }

        public void SetCloseButtonActive(bool isActive)
        {
            closeButton.gameObject.SetActive(isActive);
        }
    }
}
