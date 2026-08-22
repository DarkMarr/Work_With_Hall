using System;
using QuizGame.Ads;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MainMenu.DailyLogin
{
    public class DailyLoginController : MonoBehaviour
    {

        /// <summary>
        /// Handles the clicked event for the claim button in reward slot UI.
        /// </summary>
        /// <param name="eventData"></param>
        public void HandleSlotUIClaimClicked(DailyRewardEventData eventData)
        {
            Debug.Log($"[DailyLoginController] {eventData.RewardSlotUI.name}'s claim button clickeded");

            // CheckState for claiming
            State state = eventData.UserData.GetUpdateRewardState(eventData.UserRewardInfo);
            Debug.Log(state);
            if (state == State.LateClaimable)
            {
                if (AdsManager.Instance.IsRewardedAdAvailable())
                {
                    AdsManager.Instance.ShowRewardedAd(new AdsManager.RewardedAdShowCallbacks()
                    {
                        OnAdRewarded = (adInfo, rewardInfo) =>
                        {
                            eventData.UserData.SetUserRewardDataList(eventData.UserRewardInfo, State.Claimable);
                            eventData.RewardSlotUI.UpdateUI(eventData.UserRewardInfo);
                        }
                    });
                }
                else
                {
                    //TODO: Show some UI to inform user that ads is not available
                    Debug.Log("[DailyLoginController] Ads not available, cannot late claim reward.");
                    return;
                }
                Debug.Log(state);
            }

            if (state != State.Claimable) return;

            // Init UI
            var popUpUi = UIManager.Instance.Create<DailyRewardPopUpUI>();
            popUpUi.Init(eventData.UserRewardInfo,
                onBGClicked: () => HandlePopUpBGClicked(popUpUi),
                onClaimButtonClicked: () => HandlePopUpClaimClicked(eventData, popUpUi),
                onWatchAdsButtonClicked: (watchAdsButton) => HandlePopUpWatchAdsClicked(eventData, popUpUi, watchAdsButton, rewardMultiplier: 2));
        }

        /// <summary>
        /// Handles the clicked event for the background of the pop-up UI.
        /// </summary>
        /// <param name="popUpUI"></param>
        private void HandlePopUpBGClicked(DailyRewardPopUpUI popUpUI)
        {
            Debug.Log($"[DailyLoginController] popup background clickeded");
            popUpUI.Close();
        }

        /// <summary>
        /// Handles the clicked event for the claim button in the pop-up UI.
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="popUpUI"></param>
        private void HandlePopUpClaimClicked(DailyRewardEventData eventData, DailyRewardPopUpUI popUpUI)
        {
            Debug.Log($"[DailyLoginController] {eventData.RewardSlotUI.name}'s popup claim button clickeded");

            // Logic
            eventData.UserData.SetClaimReward(eventData.UserRewardInfo);
            popUpUI.Close();

            // Update UI
            eventData.RewardSlotUI.UpdateUI(eventData.UserRewardInfo);
        }

        /// <summary>
        /// Handles the clicked event for the watch ads button in the pop-up UI.
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="popUpUI"></param>
        /// <param name="watchAdsButton"></param>
        /// <param name="rewardMultiplier"></param>
        private void HandlePopUpWatchAdsClicked(DailyRewardEventData eventData, DailyRewardPopUpUI popUpUI, Button watchAdsButton, int rewardMultiplier)
        {
            Debug.Log($"[DailyLoginController] {eventData.RewardSlotUI.name}'s popup watchAds button clickeded");

            if (AdsManager.Instance.IsRewardedAdAvailable())
            {
                AdsManager.Instance.ShowRewardedAd(new AdsManager.RewardedAdShowCallbacks()
                {
                    OnAdRewarded = (adInfo, rewardInfo) =>
                    {
                        eventData.UserRewardInfo = eventData.UserData.SetRewardMultiplier(eventData.UserRewardInfo, rewardMultiplier);
                        watchAdsButton.interactable = false; // Disable button after watching ads

                        // Update UI
                        popUpUI.CurrentRewardSlotUI.UpdateUI(eventData.UserRewardInfo);
                        eventData.RewardSlotUI.UpdateUI(eventData.UserRewardInfo);
                    }
                });
            }
            else
            {
                //TODO: Show some UI to inform user that ads is not available
                Debug.Log("[DailyLoginController] Ads not available, cannot late claim reward.");
                return;
            }
        }
    }
}