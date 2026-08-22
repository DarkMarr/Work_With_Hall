using System;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Gameplay.UI
{
    public class GameResultScreenUI : BaseUI
    {
        public event Action onRewardButtonClicked;

        [SerializeField]
        private GameResultScreenWorldSpace gameResultWorldScreenPrefab;

        [SerializeField]
        private Button rewardButton;

        private GameResultScreenWorldSpace currentResultScreen;

        public void Init(PlayerGameResultData[] playerGameResultDatas)
        {
            currentResultScreen = Instantiate(gameResultWorldScreenPrefab, Vector3.zero, Quaternion.identity);
            currentResultScreen.Init(playerGameResultDatas);
            rewardButton.onClick.AddListener(() =>
            {
                onRewardButtonClicked?.Invoke();
                Close();
            });
        }

        public override void Close()
        {
            base.Close();
            if (currentResultScreen != null)
            {
                currentResultScreen.Close();
            }
        }
    }
}
