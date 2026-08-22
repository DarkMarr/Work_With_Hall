using System.Collections.Generic;
using UnityEngine;

namespace QuizGame.MainMenu.DailyLogin
{
    /// <summary>
    /// Represents the user's daily login data.
    /// </summary>
    public struct UserDailyLoginData
    {
        private int currentDailyLoginDate;
        public int CurrentDailyLoginDate { get => currentDailyLoginDate; }

        public List<RewardInfo> UserRewardDataList { get; set; }

        /// <summary>
        /// Determines the state of a reward based on the user's reward info and the current daily login date.
        /// </summary>
        /// <param name="userRewardInfo"></param>
        /// <returns></returns>
        public State GetUpdateRewardState(RewardInfo userRewardInfo)
        {
            if (userRewardInfo.State == State.Claimed)
                return State.Claimed;

            if (userRewardInfo.ClaimDate > CurrentDailyLoginDate)
                return State.Locked;

            if (userRewardInfo.ClaimDate < CurrentDailyLoginDate && userRewardInfo.State != State.Claimable)
                return State.LateClaimable;

            return State.Claimable;
        }

        public void SetUserRewardDataList(RewardInfo userRewardDataList, State newState)
        {
            RewardInfo foundReward = UserRewardDataList.Find(i => i.ClaimDate == userRewardDataList.ClaimDate);

            if (foundReward == null)
            {
                Debug.LogWarning($"[SetUserRewardDataList] No reward found for date: {userRewardDataList.ClaimDate}");
                return;
            }

            foundReward.State = newState;
        }

        /// <summary>
        /// Sets the current daily login date for the user.
        /// </summary>
        /// <param name="date"></param>
        public void SetCurrentDailyLoginDate(int date) => currentDailyLoginDate = date;

        /// <summary>
        /// Loads the reward data into the user's daily login data structure.
        /// </summary>
        /// <param name="infoDataList"></param>
        public void LoadRewardData(List<RewardInfo> infoDataList)
        {
            UserRewardDataList = new();

            for (int i = 0; i < infoDataList.Count; i++)
            {
                infoDataList[i].ClaimDate = i + 1; // Assuming ClaimDate is sequential starting from 1
                UserRewardDataList.Add(infoDataList[i]);
            }
        }

        /// <summary>
        /// Sets the state of a reward to 'Claimed', forcing the claim regardless of current state.
        /// </summary>
        /// <param name="dateKey"></param>
        public void SetForceClaimReward(int dateKey)
        {
            RewardInfo userRewardInfo = UserRewardDataList.Find(i => i.ClaimDate == dateKey);

            if (userRewardInfo == null)
            {
                Debug.Log($"[UserDailyLoginData] Failed to claim non-existing reward:{dateKey}");
                return;
            }

            userRewardInfo.State = State.Claimed;
        }

        /// <summary>
        /// Claims a reward for the user based on the provided UserRewardInfo.
        /// </summary>
        /// <param name="userRewardInfo"></param>
        public void SetClaimReward(RewardInfo userRewardInfo)
        {
            State state = GetUpdateRewardState(userRewardInfo);
            Debug.Log($"[UserDailyLoginData] {state.ToString()}, Reward Date: {userRewardInfo.ClaimDate}, Raward Amount:{userRewardInfo.ClaimAmount}");

            switch (state)
            {
                case State.Locked:
                    return; // Cannot claim locked reward

                case State.Claimable:
                    userRewardInfo.State = State.Claimed;
                    break;

                case State.Claimed:
                    // TODO: Claim reward here, such as giving items to player
                    break;
            }
        }

        /// <summary>
        /// Sets the reward multiplier for a user's reward.
        /// </summary>
        /// <param name="userRewardInfo"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public RewardInfo SetRewardMultiplier(RewardInfo userRewardInfo, int multiplier)
        {
            userRewardInfo.ClaimAmount *= multiplier;
            return userRewardInfo;
        }
    }

    /// <summary>
    /// Data for hold event data for daily reward slot UI interactions.
    /// </summary>
    public struct DailyRewardEventData
    {
        public UserDailyLoginData UserData;
        public DailyRewardSlotUI RewardSlotUI;
        public RewardInfo UserRewardInfo;

        public DailyRewardEventData(UserDailyLoginData userData, DailyRewardSlotUI rewardSlotUI, RewardInfo rewardInfo)
        {
            UserData = userData;
            RewardSlotUI = rewardSlotUI;
            UserRewardInfo = rewardInfo;
        }
    } 
}