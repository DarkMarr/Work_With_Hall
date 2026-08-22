using System;
using QuizGame.Localization;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MainMenu.UI
{
    public class SinglePlayerRewardUI : BaseUI
    {
        public class Data
        {
            public Sprite RewardSprite;
            public int RewardAmount;
        }

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private TextMeshProUGUI rankingText;

        [SerializeField]
        private ImageWithTextVisualization[] rewardWithAmounts;

        public void Init(Action onCloseButtonClicked, int currentRank, Data[] data)
        {
            closeButton.onClick.AddListener(() => onCloseButtonClicked?.Invoke());
            for (int i = 0; i < rewardWithAmounts.Length; i++)
            {
                var sprite = data[i].RewardSprite;
                var rewardText = $"x{data[i].RewardAmount}";
                rewardWithAmounts[i].Setup(sprite, rewardText);
            }
            rankingText.SetLocalizedArguments(currentRank);
        }
    }
}
