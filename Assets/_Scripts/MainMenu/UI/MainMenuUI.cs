using System;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("fuseButton")]
        private Button craftButton;

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

        [SerializeField, Tooltip("PlayerProfile/Currency/Coin-Image/CoinAmount-Text")]
        private TMPro.TMP_Text coinText;

        [SerializeField, Tooltip("PlayerProfile/Currency/GemAmount-Image/GemAmount-Text")]
        private TMPro.TMP_Text gemText;

        /// <summary>
        /// Shows what the player actually holds. Like the name, these counters shipped as drawings
        /// — both read 8888888 — and nothing replaced them.
        /// </summary>
        public void SetCurrency(int coins, int gems)
        {
            if (coinText != null) coinText.text = Format(coins);
            if (gemText != null) gemText.text = Format(gems);
        }

        /// <summary>
        /// Keeps the weight tag the designer put on the placeholder, so a real number is drawn the
        /// same as the mock-up was. Grouping separators are left off to match it too.
        /// </summary>
        private static string Format(int amount)
        {
            return "<font-weight=\"400\">" + amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

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
            Action onCraftButtonClicked,
            Action onMyDiaryButtonClicked,
            Action onCalendarButtonClicked,
            Action onNotificationButtonClicked,
            Action onSettingButtonClicked)
        {
            multiplayerButton.onClick.AddListener(() => onMultiplayerButtonClicked?.Invoke());
            singlePlayerButton.onClick.AddListener(() => onSinglePlayerButtonClicked?.Invoke());
            myRoomButton.onClick.AddListener(() => onMyRoomButtonClicked?.Invoke());
            storeButton.onClick.AddListener(() => onStoreButtonClicked?.Invoke());
            craftButton.onClick.AddListener(() => onCraftButtonClicked?.Invoke());
            myDiaryButton.onClick.AddListener(() => onMyDiaryButtonClicked?.Invoke());
            calendarButton.onClick.AddListener(() => onCalendarButtonClicked?.Invoke());
            notificationButton.onClick.AddListener(() => onNotificationButtonClicked?.Invoke());
            settingButton.onClick.AddListener(() => onSettingButtonClicked?.Invoke());
        }
    }
}
