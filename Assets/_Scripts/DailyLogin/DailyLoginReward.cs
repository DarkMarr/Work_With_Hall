using UnityEngine;

namespace QuizGame.MainMenu.DailyLogin
{
    /// <summary>
    /// Represents the user's reward information.
    /// </summary>
    public class RewardInfo
    {
        public Sprite ItemIconSprite;
        public int ClaimDate;
        public int ClaimAmount;

        private State _state;
        public State State
        {
            get => _state;
            set
            {
                _state = value;
                Debug.Log($"[UserRewardInfo] Set state to {value.ToString()} for reward date: {ClaimDate}");
            }
        }
    }

    public enum State
    {
        Locked,
        Claimable,
        LateClaimable,
        Claimed
    }
}