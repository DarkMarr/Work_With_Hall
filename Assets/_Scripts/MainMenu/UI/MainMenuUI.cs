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

        [Header("Player profile")]
        [SerializeField, Tooltip("PlayerProfile/Name-Text")]
        private TMPro.TMP_Text profileNameText;

        [SerializeField, Tooltip("PlayerProfile/Rank-Text")]
        private TMPro.TMP_Text rankText;

        /// <summary>
        /// Fills in the profile strip. The prefab ships with stand-ins drawn by the Art team —
        /// "namenamename", "Kindergarten |||" — and nothing replaced them, so the player saw a
        /// mock-up rather than their own name.
        /// </summary>
        public void SetProfile(string profileName, string rankName)
        {
            if (profileNameText != null && !string.IsNullOrWhiteSpace(profileName))
            {
                profileNameText.text = profileName;
            }
            if (rankText != null && !string.IsNullOrWhiteSpace(rankName))
            {
                rankText.text = rankName;
            }
        }

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
