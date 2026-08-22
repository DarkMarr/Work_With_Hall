using QuizGame.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MainMenu.DailyLogin
{
    public class DailyRewardPopUpUI : BaseUI
    {
        [SerializeField]
        private DailyRewardSlotUI currentRewardSlotUI;

        public DailyRewardSlotUI CurrentRewardSlotUI { get => currentRewardSlotUI; }

        [SerializeField]
        private Button bgButton;

        [SerializeField]
        private Button claimButton;

        [SerializeField]
        private Button watchAdsButton;

        public void Init(RewardInfo rewardInfo, Action onBGClicked, Action onClaimButtonClicked, Action<Button> onWatchAdsButtonClicked)
        {
            CurrentRewardSlotUI.UpdateUI(rewardInfo);
            bgButton.onClick.AddListener(() => onBGClicked?.Invoke());
            watchAdsButton.onClick.AddListener(() => onWatchAdsButtonClicked?.Invoke(watchAdsButton));
            claimButton.onClick.AddListener(() => onClaimButtonClicked?.Invoke());
        }
    }
}