using System;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Store.UI
{
    public class MainStoreUI : BaseUI
    {
        [SerializeField]
        private Button backButton;

        [SerializeField]
        private Button bundleStoreButton;

        [SerializeField]
        private Button avatarStoreButton;

        [SerializeField]
        private Button roomStoreButton;

        [SerializeField]
        private Button itemStoreButton;

        [SerializeField]
        private Button topUpButton;

        public void Init(
                    Action onBundleStoreButtonClicked,
                    Action onAvatarStoreButtonClicked,
                    Action onRoomStoreButtonClicked,
                    Action onItemStoreButtonClicked,
                    Action onTopUpButtonClicked,
                    Action onBackButtonClicked)
        {
            backButton.onClick.AddListener(() => onBackButtonClicked?.Invoke());
            bundleStoreButton.onClick.AddListener(() => onBundleStoreButtonClicked?.Invoke());
            avatarStoreButton.onClick.AddListener(() => onAvatarStoreButtonClicked?.Invoke());
            roomStoreButton.onClick.AddListener(() => onRoomStoreButtonClicked?.Invoke());
            itemStoreButton.onClick.AddListener(() => onItemStoreButtonClicked?.Invoke());
            topUpButton.onClick.AddListener(() => onTopUpButtonClicked?.Invoke());
        }
    }
}
