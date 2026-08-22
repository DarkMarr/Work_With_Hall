using QuizGame.Destination;
using QuizGame.MyDiary.UI;
using QuizGame.Scene;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuizGame.MyDiary
{
    public class MyDiaryController
    {
        private BaseUI currentMainUI;
        private BaseUI currentTabUI;

        public void Init(
            StatisticTabUI.Data statisticTabSetupData,
            IDestinationInfo[] travelDataDestinations)
        {
            UIManager.Instance.CloseAll();
            var myDiaryUI = UIManager.Instance.Replace<MyDiaryUI>(ref currentMainUI);
            myDiaryUI.OnCloseButtonClicked += () => HandleCloseButtonClicked();
            myDiaryUI.OnStatisticToggled += () => HandleStatisticTabToggled(statisticTabSetupData);
            myDiaryUI.OnTravelDataToggled += () => HandleTravelDataToggled(travelDataDestinations);
            myDiaryUI.Init();
        }

        private void HandleCloseButtonClicked()
        {
            Debug.Log("[MyDairy] Close button clicked");
            SceneManager.LoadScene(SceneList.MainMenu.ToString());
        }

        private void HandleStatisticTabToggled(StatisticTabUI.Data setupData)
        {
            Debug.Log("[MyDairy] Statistic Tab button clicked");

            var statisticTabUI = UIManager.Instance.Replace<StatisticTabUI>(ref currentTabUI);
            currentMainUI.AddChild(statisticTabUI);
            statisticTabUI.OnBackButtonClicked += () =>
            {
                Debug.Log("[StatisticTab] Back button clicked");
                SceneManager.LoadScene(SceneList.MainMenu.ToString());
            };
            statisticTabUI.OnRadarGraphHelpButtonClicked += () =>
            {
                var statisticHelpPopUpUI = UIManager.Instance.Create<StatisticHelpPopUpUI>();
                statisticHelpPopUpUI.Setup(detailsText: setupData.RadarHelpDetails);
            };

            statisticTabUI.Setup(setupData);
        }

        private void HandleTravelDataToggled(IDestinationInfo[] destinations)
        {
            Debug.Log("[MyDairy] Traveldata Tab button clicked");

            var travelDataTabUI = UIManager.Instance.Replace<TravelDataTabUI>(ref currentTabUI);
            travelDataTabUI.RewardInfoUI.Setup(destinations[0].GetItemRewardInDestination());
            currentMainUI.AddChild(travelDataTabUI);
            travelDataTabUI.Setup(
                avaliableDestinations: destinations,
                OnDestinationChanged: (destination) =>
                {
                    Debug.Log($"[TravelData] destination changed to: {destination}");
                    travelDataTabUI.RewardInfoUI.UpdateItems(destination.GetItemRewardInDestination());
                }
            );
        }
    }
}