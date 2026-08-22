using UnityEngine;
using System.Collections.Generic;
using QuizGame.UI;
using QuizGame.UI.Graph;
using UnityEngine.UI;
using System;

namespace QuizGame.MyDiary.UI
{
    public class StatisticTabUI : BaseUI
    {
        public event Action OnRadarGraphHelpButtonClicked;
        public event Action OnBackButtonClicked;

        [SerializeField] private Button radarGraphHelpButton;
        [SerializeField] private Button backButton;

        [SerializeField] private RadarGraph radarGraph;
        [SerializeField] private LineGraph lineGraph;

        public class Data
        {
            public float RadarStatMaxValue;
            public List<RadarGraph.StatData> RadarStatList;
            public List<string> RadarHelpDetails;
            public int RankingMaxHistory;
            public List<string> RankingLabels;
            public List<int> RankingHistory;

            public Data(
                float radarStatMaxValue,
                List<RadarGraph.StatData> radarStatList,
                List<string> radarHelpDetails,
                List<string> rankingLabels,
                int rankingMaxHistory,
                List<int> rankingHistory)
            {
                this.RadarStatMaxValue = radarStatMaxValue;
                this.RadarStatList = radarStatList;
                this.RadarHelpDetails = radarHelpDetails;
                this.RankingLabels = rankingLabels;
                this.RankingMaxHistory = rankingMaxHistory;
                this.RankingHistory = rankingHistory;
            }
        }

        private void Start()
        {
            radarGraphHelpButton.onClick.AddListener(() => OnRadarGraphHelpButtonClicked?.Invoke());
            backButton.onClick.AddListener(() => OnBackButtonClicked?.Invoke());
        }
        public void Setup(Data data)
        {
            radarGraph.Setup(
                statsList: data.RadarStatList,
                statMaxValue: data.RadarStatMaxValue
            );

            var graphSize = new Vector2Int(data.RankingMaxHistory, data.RankingLabels.Count - 1);
            lineGraph.Setup(labelList: data.RankingLabels, gridSize: graphSize, valueList: data.RankingHistory, isDrawHorizontalLine: true);
        }
        public static void ConvertRankingDataToGraph(ref List<string> labels, ref List<int> rankingStat)
        {
            // Reverse the order to match the ranking history
            labels.Reverse();

            // Convert data to index
            for (int i = 0; i < rankingStat.Count; i++)
            {
                rankingStat[i] = labels.Count - rankingStat[i];
            }
        }
    }
}