using System;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MainMenu.UI
{
    public class MainMenuUI : BaseUI
    {
        [SerializeField]
        private Button multiplayerButton;

        [SerializeField]
        private Button singlePlayerButton;

        [SerializeField]
        private Button myRoomButton;

        [SerializeField]
        private Button storeButton;

        [SerializeField]
        private Button fuseButton;

        [SerializeField]
        private Button myDiaryButton;

        [SerializeField]
        private Button calendarButton;

        [SerializeField]
        private Button notificationButton;

        [SerializeField]
        private Button settingButton;

        public void Init(
            Action onMultiplayerButtonClicked,
            Action onSinglePlayerButtonClicked,
            Action onMyRoomButtonClicked,
            Action onStoreButtonClicked,
            Action onFuseButtonClicked,
            Action onMyDiaryButtonClicked,
            Action onCalendarButtonClicked,
            Action onNotificationButtonClicked,
            Action onSettingButtonClicked)
        {
            multiplayerButton.onClick.AddListener(() => onMultiplayerButtonClicked?.Invoke());
            singlePlayerButton.onClick.AddListener(() => onSinglePlayerButtonClicked?.Invoke());
            myRoomButton.onClick.AddListener(() => onMyRoomButtonClicked?.Invoke());
            storeButton.onClick.AddListener(() => onStoreButtonClicked?.Invoke());
            fuseButton.onClick.AddListener(() => onFuseButtonClicked?.Invoke());
            myDiaryButton.onClick.AddListener(() => onMyDiaryButtonClicked?.Invoke());
            calendarButton.onClick.AddListener(() => onCalendarButtonClicked?.Invoke());
            notificationButton.onClick.AddListener(() => onNotificationButtonClicked?.Invoke());
            settingButton.onClick.AddListener(() => onSettingButtonClicked?.Invoke());
        }
    }
}
