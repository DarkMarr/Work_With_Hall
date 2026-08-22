using System;
using QuizGame.MyRoom.FriendList.UI;
using QuizGame.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace QuizGame.MyRoom.FriendList
{
    [Serializable]
    public class FriendListController
    {
        [SerializeField]
        private FriendListLocalizationDataSO localization;

        private FriendListModel model;
        private FriendListUI friendListUI;
        private BaseUI currentFriendUI;

        public void Init(FriendListModel friendListModel)
        {
            model = friendListModel;
            ShowFriendListUI();
        }

        private void ShowFriendListUI()
        {
            friendListUI = UIManager.Instance.Replace<FriendListUI>(ref currentFriendUI);
            friendListUI.Init(
                friendAmount: model.GetFriendAmount(),
                onCloseButtonClicked: friendListUI.Close,
                onRemoveButtonClicked: HandleRemoveButtonClicked,
                onBackButtonClicked: HandleBackButtonClicked,
                onRequestedButtonClicked: HandleRequestButtonClicked,
                onAddButtonClicked: HandleAddButtonClicked,
                onFriendElementUpdate: OnFriendListElementUpdate
            );
            friendListUI.ShowHomeMode();
        }

        private void HandleAddButtonClicked()
        {
            var popup = UIManager.Instance.Create<EnterTextPopupUI>(friendListUI);
            popup.Setup(
                title: localization.EnterUIDLocalized.GetLocalizedString(),
                onCloseButtonClicked: popup.Close,
                onConfirmButtonClicked: (enteredUID) =>
                {
                    SearchingForUID(enteredUID);
                    popup.Close();
                });
        }

        private void SearchingForUID(string uid)
        {
            Debug.Log($"[{GetType().Name}] Search for UID: {uid}");

            var foundedFriend = new FriendData(uid, "NewFriend", "Master"); //TODO: [Network] Request friend data by service
            var isFound = Random.Range(0, 2) == 1; //TODO: [Network] Connect to real network service
            var popup = UIManager.Instance.Create<MessagePopupUI>(friendListUI);
            if (isFound)
            {
                Debug.Log($"[{GetType().Name}] Found UID: {uid}");
                popup.Setup(
                    title: $"UID: {uid}",
                    description: localization.ShowNameLocalized.GetLocalizedString(foundedFriend.Name), //$"Name: {foundedFriend.Name}",
                    buttonMessage: localization.AddLocalized.GetLocalizedString(),
                    onMessageButtonClicked: () =>
                    {
                        AddFriend(uid);
                        popup.Close();
                    },
                    onCloseButtonClicked: popup.Close);
            }
            else
            {
                Debug.Log($"[{GetType().Name}] Cannot find UID: {uid}");
                popup.Setup(
                    title: localization.CannotFindLocalized.GetLocalizedString(),
                    description: $"UID: {uid}",
                    buttonMessage: localization.BackLocalized.GetLocalizedString(),
                    onMessageButtonClicked: popup.Close,
                    onCloseButtonClicked: popup.Close);
            }
        }

        private void AddFriend(string uid)
        {
            Debug.Log($"[{GetType().Name}] Add friend UID: {uid}");
            var resultMessage = localization.RequestSentLocalized.GetLocalizedString(); //TODO: [Network] Repalce with real service message
            var popup = UIManager.Instance.Create<MessagePopupUI>(friendListUI);
            popup.Setup(
                title: $"UID: {uid}",
                description: resultMessage,
                buttonMessage: localization.OkayLocalized.GetLocalizedString(),
                onMessageButtonClicked: popup.Close,
                onCloseButtonClicked: null);
        }

        private void OnFriendListElementUpdate(int index, FriendListElement element)
        {
            var friend = model.GetFriendAtIndex(index);
            element.Setup(
                name: friend.Name,
                rank: friend.Rank,
                onHomeButtonClicked: () => VisitFriend(friend),
                onDeleteButtonClicked: () => DeleteFriend(friend));
        }

        private void HandleRequestButtonClicked()
        {
            var friendRequestedUI = UIManager.Instance.Replace<FriendRequestedUI>(ref currentFriendUI);
            friendRequestedUI.Init(
                requestedAmount: model.GetFriendAmount(),
                onBackButtonClicked: () =>
                {
                    friendRequestedUI.Close();
                    ShowFriendListUI();
                },
                onFriendRequestedElementUpdate: (dataIndex, element) => OnFriendRequestedElementUpdate(friendRequestedUI, dataIndex, element));
        }

        private void OnFriendRequestedElementUpdate(FriendRequestedUI friendRequestedUI, int dataIndex, FriendRequestElement element)
        {
            var requestedFriend = model.GetFriendRequestedAtIndex(dataIndex);
            element.Setup(
                name: requestedFriend.Name,
                rank: requestedFriend.Rank,
                onAcceptButtonClicked: () =>
                {
                    Debug.Log($"{GetType().Name} Accept friend requested name: {requestedFriend.Name}");
                    model.AddFriend(requestedFriend); //TODO: [Network] Add requested friend to database
                    DeleteFriendRequest(friendRequestedUI, requestedFriend);
                },
                onRejectButtonClicked: () =>
                {
                    Debug.Log($"{GetType().Name} Reject friend requested name: {requestedFriend.Name}");
                    DeleteFriendRequest(friendRequestedUI, requestedFriend);
                }
            );
        }

        private void DeleteFriendRequest(FriendRequestedUI friendRequestedUI, FriendData requestedFriend)
        {
            model.DeleteFriendRequested(requestedFriend);
            friendRequestedUI.UpdateFriendRequestedListView(model.GetFriendAmount());
        }

        private void HandleRemoveButtonClicked()
        {
            friendListUI.ShowDeleteMode();
        }

        private void HandleBackButtonClicked()
        {
            friendListUI.ShowHomeMode();
        }

        private void VisitFriend(FriendData friend)
        {
            Debug.Log($"Going to friend home ID: {friend.UID}");
        }

        private void DeleteFriend(FriendData friend)
        {
            var popup = UIManager.Instance.Create<ConfirmPopupUI>(friendListUI);
            popup.Setup(
                title: localization.RemoveLocalized.GetLocalizedString(),
                description: localization.ShowNameLocalized.GetLocalizedString(friend.Name),
                onConfirmButtonClicked: () =>
                {
                    Debug.Log($"Delete friend ID: {friend.UID}");
                    model.DeleteFriend(friend);
                    friendListUI.UpdateFriendListView(model.GetFriendAmount());
                    popup.Close();
                },
                onCancelButtonClicked: () =>
                {
                    popup.Close();
                }
            );
        }
    }
}
