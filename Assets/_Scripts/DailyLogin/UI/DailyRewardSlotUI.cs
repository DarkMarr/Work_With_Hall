using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MainMenu.DailyLogin
{
    public class DailyRewardSlotUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI rewardDateText;

        [SerializeField]
        private Image itemIconImage;

        [SerializeField]
        private Image claimedIconImage;

        [SerializeField]
        private Image lateClaimedIconImage;

        [SerializeField]
        private Image lockedIconImage;

        [SerializeField]
        private TextMeshProUGUI claimAmountText;

        [SerializeField]
        private Button claimButton;

        public void Init(RewardInfo rewardInfo, Action onClaimButtonClicked)
        {
            UpdateUI(rewardInfo);

            claimButton.onClick.AddListener(() => onClaimButtonClicked?.Invoke());
        }

        /// <summary>
        /// Updates state ui and info labels of the reward slot.
        /// </summary>
        /// <param name="rewardInfo"></param>
        public void UpdateUI(RewardInfo rewardInfo)
        {
            UpdateUISlotInfo(rewardInfo);
            UpdateStateUI(rewardInfo);
        }

        /// <summary>
        /// Updates the UI slot information textlabel.
        /// </summary>
        /// <param name="rewardInfo"></param>
        private void UpdateUISlotInfo(RewardInfo rewardInfo)
        {
            rewardDateText.text = rewardInfo.ClaimDate.ToString("Day0");
            itemIconImage.sprite = rewardInfo.ItemIconSprite;
            claimAmountText.text = rewardInfo.ClaimAmount.ToString("'x'0");
        }

        /// <summary>
        /// Updates the state icons image.
        /// </summary>
        /// <param name="userRewardInfo"></param>
        private void UpdateStateUI(RewardInfo userRewardInfo)
        {
            State state = userRewardInfo.State;

            lockedIconImage.gameObject.SetActive(state == State.Locked);
            claimedIconImage.gameObject.SetActive(state == State.Claimed);
            lateClaimedIconImage.gameObject.SetActive(state == State.LateClaimable);
        }
    }
}