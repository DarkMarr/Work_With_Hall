using QuizGame.MainMenu.DailyLogin;
using QuizGame.Scene;
using QuizGame.UI;
using QuizGame.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace QuizGame.MainMenu.UI
{
    public class DailyLoginUI : BaseUI
    {
        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private Transform claimDailyUIContainer;

        [SerializeField]
        private DailyRewardSlotUI dailyRewardSlotUIPrefab;

        private List<DailyRewardSlotUI> dailyRewardSlotList;

        private void Start()
        {
            closeButton.onClick.AddListener(() => HandleMenuButtonClicked());
        }

        public void Init(Action onCloseButtonClicked)
        {
            closeButton.onClick.AddListener(() => onCloseButtonClicked?.Invoke());
        }

        /// <summary>
        /// Initializes the DailyLoginController and sets up the daily reward slot list.
        /// </summary>
        /// <param name="userData"></param>
        public void CreateDailyRewardsSlotUI(UserDailyLoginData userData, Action<DailyRewardEventData> onRewardSlotClicked)
        {
            ClearDailyRewardsUI();

            foreach (RewardInfo rewardInfo in userData.UserRewardDataList)
            {
                rewardInfo.State = userData.GetUpdateRewardState(rewardInfo);

                DailyRewardSlotUI rewardSlot = Instantiate(dailyRewardSlotUIPrefab, claimDailyUIContainer);
                DailyRewardEventData rewardSlotEventData = new DailyRewardEventData(userData, rewardSlot, rewardInfo);

                rewardSlot.Init(rewardInfo, () => onRewardSlotClicked(rewardSlotEventData));
                dailyRewardSlotList.Add(rewardSlot);
            }
        }

        /// <summary>
        /// Clears the daily rewards UI by destroying all existing reward slot UIs and resetting the list.
        /// </summary>
        private void ClearDailyRewardsUI()
        {
            claimDailyUIContainer.DoActionOnChildren((child) => Destroy(child.gameObject));
            dailyRewardSlotList = new();
        }

        private void HandleMenuButtonClicked()
        {
            SceneManager.LoadScene(SceneList.MainMenu.ToString());
        }
    }
}