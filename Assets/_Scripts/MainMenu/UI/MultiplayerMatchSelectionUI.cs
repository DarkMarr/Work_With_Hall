using System;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MainMenu.UI
{
    public class MultiplayerMatchSelectionUI : BaseUI
    {
        public event Action OnRankingMatchClicked;
        public event Action OnCasualMatchClicked;
        public event Action OnLeaderboardClicked;
        public event Action OnBackClicked;

        [SerializeField]
        private Button rankingMatchButton;

        [SerializeField]
        private Button casualMatchButton;

        [SerializeField]
        private Button leaderboardButton;

        [SerializeField]
        private Button backButton;

        private void Start()
        {
            rankingMatchButton.onClick.AddListener(() => OnRankingMatchClicked?.Invoke());
            casualMatchButton.onClick.AddListener(() => OnCasualMatchClicked?.Invoke());
            leaderboardButton.onClick.AddListener(() => OnLeaderboardClicked?.Invoke());
            backButton.onClick.AddListener(() =>
            {
                OnBackClicked?.Invoke();
                Close();
            });
        }
    }
}
