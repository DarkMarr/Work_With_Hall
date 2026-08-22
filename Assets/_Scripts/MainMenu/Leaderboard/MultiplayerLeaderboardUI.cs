using System;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MainMenu.Leaderboard
{
    public class MultiplayerLeaderboardUI : BaseUI
    {
        public event Action OnBackButtonClicked;

        [SerializeField]
        private LeaderboardView leaderboardView;

        [SerializeField]
        private Button backButton;

        [SerializeField]
        private TextMeshProUGUI currentRankingText;

        private LeaderboardData[] leaderboardDatas;

        private bool isInit;

        void Start()
        {
            OnClosed += OnBackButtonClicked;
            backButton.onClick.AddListener(Close);
        }

        public void Init(LeaderboardData[] leaderboardDatas, int currentRankNumber)
        {
            if (isInit)
            {
                Debug.LogWarning("MultiplayerLeaderboardUI is already initialized. Call UpdateLeaderboard() to refresh data.");
                return;
            }
            this.leaderboardDatas = leaderboardDatas;
            leaderboardView.Init(leaderboardDatas.Length, OnElementUpdate);
            currentRankingText.text = $"Current Ranking - {currentRankNumber}";
            isInit = true;
        }

        public void UpdateLeaderboard(LeaderboardData[] leaderboardDatas, int currentRankNumber)
        {
            if (!isInit)
            {
                Debug.LogWarning("MultiplayerLeaderboardUI is not initialized. Call Init() before updating leaderboard.");
                return;
            }
            this.leaderboardDatas = leaderboardDatas;
            leaderboardView.UpdateUI(leaderboardDatas.Length);
            currentRankingText.text = $"Current Ranking - {currentRankNumber}";
        }

        private void OnElementUpdate(int dataIndex, LeaderboardElement element)
        {
            var data = leaderboardDatas[dataIndex];
            element.Setup(data.RankNumber, data.PlayerName, data.RankName, data.RankScore);
        }
    }
}
