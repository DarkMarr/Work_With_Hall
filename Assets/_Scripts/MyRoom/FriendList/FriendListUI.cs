using System;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MyRoom.FriendList.UI
{
    public class FriendListUI : BaseUI
    {
        [SerializeField]
        private FriendListView friendListView;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private Button backButton;

        [Header("Friend action button")]
        [SerializeField]
        private Transform friendActionButtonContainer;

        [SerializeField]
        private Button addButton;

        [SerializeField]
        private Button removeButton;

        [SerializeField]
        private Button requestedButton;

        public void Init(
            int friendAmount,
            Action onCloseButtonClicked,
            Action onRemoveButtonClicked,
            Action onBackButtonClicked,
            Action onRequestedButtonClicked,
            Action onAddButtonClicked,
            FriendListView.OnElementDataUpdateHandler onFriendElementUpdate)
        {
            closeButton.onClick.AddListener(() => onCloseButtonClicked?.Invoke());
            removeButton.onClick.AddListener(() => onRemoveButtonClicked?.Invoke());
            backButton.onClick.AddListener(() => onBackButtonClicked?.Invoke());
            requestedButton.onClick.AddListener(() => onRequestedButtonClicked?.Invoke());
            addButton.onClick.AddListener(() => onAddButtonClicked?.Invoke());
            friendListView.Init(
                dataAmount: friendAmount,
                onElementDataUpdate: onFriendElementUpdate
            );
        }

        public void UpdateFriendListView(int friendAmount)
        {
            friendListView.UpdateUI(friendAmount);
        }

        public void ShowHomeMode()
        {
            friendListView.SetElementsToHomeMode();
            backButton.gameObject.SetActive(false);
            friendActionButtonContainer.gameObject.SetActive(true);
        }

        public void ShowDeleteMode()
        {
            friendListView.SetElementsToDeleteMode();
            backButton.gameObject.SetActive(true);
            friendActionButtonContainer.gameObject.SetActive(false);
        }
    }
}
