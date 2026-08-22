using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.UI
{
    public class EnterTextPopupUI : BaseUI
    {
        [SerializeField]
        private Button confirmButton;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField]
        private TMP_InputField inputField;

        private event Action<string> onConfirmButtonClicked;
        private event Action onCloseButtonClicked;

        protected override void Awake()
        {
            base.Awake();
            confirmButton.onClick.AddListener(() => onConfirmButtonClicked?.Invoke(inputField.text));
            closeButton.onClick.AddListener(() => onCloseButtonClicked?.Invoke());
        }

        public void Setup(string title, Action onCloseButtonClicked, Action<string> onConfirmButtonClicked)
        {
            titleText.text = title;
            this.onConfirmButtonClicked = onConfirmButtonClicked;
            this.onCloseButtonClicked = onCloseButtonClicked;
            Show();
        }
    }
}
