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

        [Header("Header capsule")]
        [SerializeField]
        [Tooltip("The capsule behind the title. Leave empty to keep whatever the prefab shows.")]
        private Image titleBackground;

        [SerializeField]
        private Sprite noticeHeader;

        [SerializeField]
        private Sprite alertHeader;

        private event Action onMessageButtonClicked;
        private event Action onCloseButtonClicked;

        protected override void Awake()
        {
            base.Awake();
            messageButton.onClick.AddListener(() => onMessageButtonClicked?.Invoke());
            closeButton.onClick.AddListener(() => onCloseButtonClicked?.Invoke());
        }

        /// <param name="tone">Red for something that did not happen, green otherwise. See
        /// <see cref="PopupTone"/>. Defaults to green so existing callers keep their look.</param>
        public void Setup(string title, string description, string buttonMessage, Action onMessageButtonClicked, Action onCloseButtonClicked = null, PopupTone tone = PopupTone.Notice)
        {
            SetTone(tone);
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

        /// <summary>
        /// Swaps the capsule behind the title. Silently does nothing when the prefab has no
        /// capsule or no sprite for the tone, so a popup that has not been dressed yet still
        /// shows its message rather than losing its header.
        /// </summary>
        public void SetTone(PopupTone tone)
        {
            if (titleBackground == null) return;
            var header = tone == PopupTone.Alert ? alertHeader : noticeHeader;
            if (header != null) titleBackground.sprite = header;
        }

        public void SetCloseButtonActive(bool isActive)
        {
            closeButton.gameObject.SetActive(isActive);
        }
    }
}
