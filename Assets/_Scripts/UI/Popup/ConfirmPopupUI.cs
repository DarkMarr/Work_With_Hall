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

        [Header("Header capsule")]
        [SerializeField]
        [Tooltip("The capsule behind the title. Leave empty to keep whatever the prefab shows.")]
        private Image titleBackground;

        [SerializeField]
        private Sprite noticeHeader;

        [SerializeField]
        private Sprite alertHeader;

        private event Action onConfirmButtonClicked;
        private event Action onCancelButtonClicked;

        protected override void Awake()
        {
            base.Awake();
            confirmButton.onClick.AddListener(() => onConfirmButtonClicked?.Invoke());
            cancelButton.onClick.AddListener(() => onCancelButtonClicked?.Invoke());
        }

        /// <param name="tone">Red when confirming would destroy or spend something the player
        /// cannot get back. See <see cref="PopupTone"/>.</param>
        public void Setup(string title, string description, Action onConfirmButtonClicked, Action onCancelButtonClicked, PopupTone tone = PopupTone.Notice)
        {
            SetTone(tone);
            TitleText.text = title;
            descriptionText.text = description;
            this.onConfirmButtonClicked = onConfirmButtonClicked;
            this.onCancelButtonClicked = onCancelButtonClicked;
            Show();
        }

        /// <summary>Swaps the capsule behind the title; see <see cref="MessagePopupUI.SetTone"/>.</summary>
        public void SetTone(PopupTone tone)
        {
            if (titleBackground == null) return;
            var header = tone == PopupTone.Alert ? alertHeader : noticeHeader;
            if (header != null) titleBackground.sprite = header;
        }
    }
}
