using System;
using QuizGame.MyRoom.FriendList.UI;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestedUI : BaseUI
{
    [SerializeField]
    private FriendRequestedView friendRequestedView;

    [SerializeField]
    private Button backButton;

    private Action onBackButtonClicked;

    protected override void Awake()
    {
        base.Awake();
        backButton.onClick.AddListener(() => onBackButtonClicked?.Invoke());
    }

    public void Init(
            int requestedAmount,
            Action onBackButtonClicked,
            FriendRequestedView.OnElementDataUpdateHandler onFriendRequestedElementUpdate)
    {
        this.onBackButtonClicked = onBackButtonClicked;
        friendRequestedView.Init(
            dataAmount: requestedAmount,
            onElementDataUpdate: onFriendRequestedElementUpdate
        );
    }

    public void UpdateFriendRequestedListView(int friendRequestedAmount)
    {
        friendRequestedView.UpdateUI(friendRequestedAmount);
    }
}
