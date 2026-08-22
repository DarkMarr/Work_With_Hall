using System.Collections.Generic;
using System.Linq;
using QuizGame.Destination;
using QuizGame.MainMenu.DailyLogin;
using QuizGame.MainMenu.UI;
using QuizGame.Matchmaking;
using QuizGame.MyDiary.UI;
using QuizGame.Player;
using QuizGame.Setting;
using QuizGame.Scene;
using QuizGame.Store;
using QuizGame.UI;
using QuizGame.UI.Graph;
using UnityEngine;
using UnityEngine.SceneManagement;
using QuizGame.Network;
using Newtonsoft.Json;
using QuizGame.Item.Interfaces;
using QuizGame.MainMenu.Leaderboard;
using QuizGame.MyDiary;
using QuizGame.Fuse;
using QuizGame.Gameplay.QuizManagement;
using QuizGame.Gameplay;
using QuizGame.Item;

namespace QuizGame.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        public enum BackgroundType
        {
            Room,
            Matching
        }

        [SerializeField]
        private DailyLoginController dailyLoginController;

        [SerializeField]
        private GameObject roomBackground;

        [SerializeField]
        private GameObject matchingBackground;

        [SerializeField]
        private MatchmakingController matchmakingController;

        private StoreController storeController = new StoreController();
        private MyDiaryController myDiaryController = new MyDiaryController();
        private FuseController fuseController = new FuseController();
        private SettingController settingController = new SettingController();

        private MainMenuProfileUI profileUI;
        private BaseUI currentMainUI;

        //TODO: Mock up only
        private float tilEnergyRechargeMS = 2990;

        private async void Start()
        {
            UIManager.Instance.CloseAll();

            var profileData = await PlayerDataManager.Instance.GetProfileData();
            var energyData = await PlayerDataManager.Instance.GetEnergyData();

            profileUI = UIManager.Instance.Create<MainMenuProfileUI>();
            profileUI.SetProfileName(profileData?.ProfileName ?? "Unknown");
            profileUI.SetRank("Unranked");
            profileUI.SetEnergy(energyData?.Current ?? 0, energyData?.Max ?? 0);
            ShowMainMenu();

            matchmakingController.InjectUIManager(UIManager.Instance);
            matchmakingController.OnExitMatchmaking += HandleMultiplayerMatchSelectionClicked;
        }

        private void Update()
        {
            //TODO: Mock up only, we may need energy system/controller later
            if (profileUI != null)
            {
                tilEnergyRechargeMS -= Time.deltaTime;
                profileUI.SetTimer(Mathf.RoundToInt(tilEnergyRechargeMS));
            }

            if (Input.GetKeyDown(KeyCode.Escape) && matchmakingController.CurrentState == MatchmakingState.Preparing)
            {
                matchmakingController.ExitMatchMaking();
            }
        }

        private void ShowMainMenu()
        {
            profileUI?.Show();

            var mainMenuUI = UIManager.Instance.Replace<MainMenuUI>(ref currentMainUI);
            mainMenuUI.Init(
                onMultiplayerButtonClicked: HandleMultiplayerMatchSelectionClicked,
                onSinglePlayerButtonClicked: HandleSinglePlayerButtonClicked,
                onMyRoomButtonClicked: HandleMyRoomButtonClicked,
                onStoreButtonClicked: HandleStoreButtonClicked,
                onFuseButtonClicked: HandleFuseButtonClicked,
                onMyDiaryButtonClicked: HandleMyDiaryButtonClicked,
                onCalendarButtonClicked: HandleCalendarButtonClicked,
                onNotificationButtonClicked: HandleNotificationButtonClicked,
                onSettingButtonClicked: HandleSettingButtonClicked
            );
            SwitchBackground(BackgroundType.Room);
        }

        private void SwitchBackground(BackgroundType type)
        {
            roomBackground.SetActive(type == BackgroundType.Room);
            matchingBackground.SetActive(type == BackgroundType.Matching);
        }

        private void HandleMultiplayerMatchSelectionClicked()
        {
            var multiplayerMatchSelectionUI = UIManager.Instance.Replace<MultiplayerMatchSelectionUI>(ref currentMainUI);
            multiplayerMatchSelectionUI.OnRankingMatchClicked += OnRankingMatchClicked;
            multiplayerMatchSelectionUI.OnCasualMatchClicked += OnCasualMatchClicked;
            multiplayerMatchSelectionUI.OnLeaderboardClicked += OnLeaderboardClicked;
            multiplayerMatchSelectionUI.OnBackClicked += ShowMainMenu;
            SwitchBackground(BackgroundType.Room);
        }

        private void OnRankingMatchClicked()
        {
            SwitchBackground(BackgroundType.Matching);
            StartMatchmaking(MatchmakingType.Ranking);
        }

        private void OnCasualMatchClicked()
        {
            SwitchBackground(BackgroundType.Matching);
            StartMatchmaking(MatchmakingType.Casual);
        }

        private void OnLeaderboardClicked()
        {
            var leaderboard = UIManager.Instance.Create<MultiplayerLeaderboardUI>();
            var leaderboardDatas = JsonConvert.DeserializeObject<LeaderboardData[]>(LeaderboardData.GetTempJsonData()); //TODO: [Network] Load real data
            var currentPlayerRank = 43; //TODO: [Network] Load real data
            leaderboard.Init(leaderboardDatas, currentPlayerRank);
        }

        private void StartMatchmaking(MatchmakingType matchType)
        {
            profileUI.Hide();
            var availableDestinations = DestinationResourceManager.Instance.GetAllResources();
            var availableItems = GetPlayerCarryOnItems();
            matchmakingController.OpenMatchmakingSequence(matchType, availableItems, availableDestinations, ref currentMainUI, 
                onMatchmakingCompleted : (success) =>
                {
                    if (success)
                    {
                        GameplayController.SelectedDestinationInfo = matchmakingController.SelectedDestinationInfo;
                        GameplayController.CurrentGameMode = GameplayController.GameMode.Multiplayer;
                    }
                });
        }


        public ItemWithQuantityPair[] GetPlayerCarryOnItems()
        {
            var allItems = CarryOnItemResourceManager.Instance.GetAllResources(); // TODO: [Network] Replace with actual data
            var ItemWithQuantityPairs = new ItemWithQuantityPair[allItems.Length];
            for (int i = 0; i < allItems.Length; i++)
            {
                var itemAmount = Random.Range(1, 256); // TODO: [Network] Use actual quantity
                ItemWithQuantityPairs[i] = new ItemWithQuantityPair(allItems[i], itemAmount);
            }
            return ItemWithQuantityPairs;
        }

        #region Single Player Button Handlers

        private void HandleSinglePlayerButtonClicked()
        {
            var singlePlayerMatchSelectionUI = UIManager.Instance.Replace<SinglePlayerMatchSelectionUI>(ref currentMainUI);
            singlePlayerMatchSelectionUI.OnLibraryButtonClicked += HandleLibraryButtonClicked;
            singlePlayerMatchSelectionUI.OnPlaygroundButtonClicked += HandlePlaygroundButtonClicked;
            singlePlayerMatchSelectionUI.OnLeaderboardButtonClicked += HandleLeaderboardButtonClicked;
            singlePlayerMatchSelectionUI.OnBackButtonClicked += ShowMainMenu;
            SwitchBackground(BackgroundType.Room);
        }

        private void HandleLibraryButtonClicked()
        {
            var libraryQuizCategory = new List<QuizCategory>() { QuizCategory.General, QuizCategory.Geography, QuizCategory.History, QuizCategory.Science };  //TODO: Make some kind of game settings might be better practice.
            GameplayController.SelectedQuizCategories = libraryQuizCategory;
            GameplayController.CurrentGameMode = GameplayController.GameMode.SinglePlayer;
            SceneManager.LoadScene(SceneList.Gameplay.ToString());
        }

        private void HandlePlaygroundButtonClicked()
        {
            var playgroundQuizCategory = new List<QuizCategory>() { QuizCategory.General, QuizCategory.PopCulture, QuizCategory.Entertainment, QuizCategory.Sports };  //TODO: Make some kind of game settings might be better practice.
            GameplayController.SelectedQuizCategories = playgroundQuizCategory;
            GameplayController.CurrentGameMode = GameplayController.GameMode.SinglePlayer;
            SceneManager.LoadScene(SceneList.Gameplay.ToString());
        }

        private void HandleLeaderboardButtonClicked()
        {
            var leaderboard = UIManager.Instance.Create<SinglePlayerLeaderboardUI>();
            SetupLeaderboardUI(leaderboard);
        }

        private void ShowSinglePlayerRewardUI()
        {
            var rewardInformation = new SinglePlayerRewardUI.Data[]
            {
                new SinglePlayerRewardUI.Data { RewardSprite = null, RewardAmount = 1 },
                new SinglePlayerRewardUI.Data { RewardSprite = null, RewardAmount = 3 },
                new SinglePlayerRewardUI.Data { RewardSprite = null, RewardAmount = 7 },
            };

            var singlePlayerRewardUI = UIManager.Instance.Replace<SinglePlayerRewardUI>(ref currentMainUI);
            singlePlayerRewardUI.Init(
                onCloseButtonClicked: HandleSinglePlayerButtonClicked,
                currentRank: 317,
                data: rewardInformation
            );
        }

        private void SetupLeaderboardUI(SinglePlayerLeaderboardUI leaderboard)
        {
            var leaderboardDatas = JsonConvert.DeserializeObject<LeaderboardData[]>(LeaderboardData.GetTempJsonData());
            var currentPlayerRank = 43;
            leaderboard.Init(leaderboardDatas, currentPlayerRank);

            var categoryNames = new string[] { "General Knowledge", "General", "Science", "History" };
            leaderboard.SetupDropdownOptions(categoryNames);

            leaderboard.OnCategoryChanged += (selectIndex) =>
            {
                Debug.Log($"[MainMenu] User select category index: {selectIndex}, Name: {categoryNames[selectIndex]}");
                var newLeaderboardDatas = JsonConvert.DeserializeObject<LeaderboardData[]>(LeaderboardData.GetTempJsonData());
                var newCurrentPlayerRank = 43;
                leaderboard.UpdateLeaderboard(newLeaderboardDatas, newCurrentPlayerRank);
            };
        }

        #endregion

        private void HandleMyRoomButtonClicked()
        {
            SceneManager.LoadScene(SceneList.MyRoom.ToString());
        }

        private void HandleStoreButtonClicked()
        {
            currentMainUI.Close();
            profileUI.Hide();
            storeController.OpenMainStore();
        }

        private void HandleFuseButtonClicked()
        {
            Debug.Log("[MainMenu] Fuse button clicked");

            // TODO: Replace with real data from database  
            // Temp data - PlayerMaterial
            var playerMaterials = PlayerMaterial.FromJson(PlayerMaterial.GetMaterialsDataTempDataJson());
            var playerModel = new FusingPlayerModel(playerMaterials: playerMaterials);
            fuseController.Setup(fuseTabModelList: FuseTabModel.FuseTabList, playerModel: playerModel);
        }

        private void HandleMyDiaryButtonClicked()
        {
            Debug.Log("[MainMenu] My Diary button clicked");

            // TODO: Replace with real data from database  
            // Temp data - Radar chart data  
            var radarStatList = new List<RadarGraph.StatData>()
            {
                   new() { Name = "Stat 1", Value = 10, QuestionType = "Temp" },
                   new() { Name = "Stat 2", Value = 5, QuestionType = "Temp" },
                   new() { Name = "Stat 3", Value = 5, QuestionType = "Temp" },
                   new() { Name = "Stat 4", Value = 0, QuestionType = "Temp" },
                   new() { Name = "Stat 5", Value = 5, QuestionType = "Temp" },
                   new() { Name = "Stat 6", Value = 5, QuestionType = "Temp" },
                   new() { Name = "Stat 7", Value = 5, QuestionType = "Temp" },
                   new() { Name = "Stat 8", Value = 5, QuestionType = "Temp" }
            };
            var radarStatisticHelpPopUpDetailText = new List<string>();
            foreach (var stat in radarStatList)
            {
                radarStatisticHelpPopUpDetailText.Add(
                    $"{stat.Name} - Increase with your accuracy of lastest \n " +
                    $"{stat.Value} answers in \"{stat.QuestionType}\" question type");
            }
            var radarStatMaxValue = 10;

            // TODO: Replace with real data from database  
            // Temp data - Match trending data  
            var rankingLabels = new List<string> { "1st", "2nd", "3rd", "4th" };
            var maxRankingHistory = 10;
            var rankingHistoryStat = new List<int> { 1, 2, 3, 4, 2, 3 };

            var statisticTabData = new StatisticTabUI.Data(
                radarStatMaxValue,
                radarStatList: radarStatList,
                radarHelpDetails: radarStatisticHelpPopUpDetailText,
                rankingLabels: rankingLabels,
                rankingMaxHistory: maxRankingHistory,
                rankingHistory: rankingHistoryStat
                );

            StatisticTabUI.ConvertRankingDataToGraph(labels: ref rankingLabels, ref rankingHistoryStat);

            var destinations = DestinationResourceManager.Instance.GetAllResources();

            myDiaryController.Init(
                statisticTabSetupData: statisticTabData,
                travelDataDestinations: destinations);
        }

        private void HandleCalendarButtonClicked()
        {
            Debug.Log("[MainMenu] Calendar button clicked");

            // Temp data
            var userDailyLoginData = new UserDailyLoginData();

            //Temp Data
            List<RewardInfo> tempDailyRewardData = new()
            {
                new() {ItemIconSprite = default, ClaimAmount  = 1},
                new() {ItemIconSprite = default, ClaimAmount = 2},
                new() {ItemIconSprite = default, ClaimAmount = 2},
                new() {ItemIconSprite = default, ClaimAmount = 1},
                new() {ItemIconSprite = default, ClaimAmount = 5},
            };

            // Changing data
            userDailyLoginData.LoadRewardData(tempDailyRewardData);
            userDailyLoginData.SetCurrentDailyLoginDate(3);
            userDailyLoginData.SetForceClaimReward(2);

            // UI
            profileUI.Hide();
            var dailyLoginUI = UIManager.Instance.Replace<DailyLoginUI>(ref currentMainUI);

            dailyLoginUI.Init(onCloseButtonClicked: () => ShowMainMenu());
            dailyLoginUI.CreateDailyRewardsSlotUI(userDailyLoginData, dailyLoginController.HandleSlotUIClaimClicked);
        }

        private void HandleNotificationButtonClicked()
        {
            var systemMessageUI = UIManager.Instance.Replace<SystemMessageUI>(ref currentMainUI);
            //TODO: [Network] Load real message datas
            var systemMessageDatas = JsonConvert.DeserializeObject<SystemMessageData[]>(SystemMessageData.GetTempJsonData());
            systemMessageUI.Init(systemMessageDatas);
            systemMessageUI.OnBackButtonClicked += () => ShowMainMenu();
            systemMessageUI.OnUserAcceptGivingItems += GiveItemsToPlayer;
        }

        private void GiveItemsToPlayer(IQuantifiableItem[] items)
        {
            foreach (var item in items)
            {
                Debug.Log($"Player has take giving item ID: {item.GetID()}, Type Number: {(int)item.GetItemType()}, Quantity: {item.GetQuantity()}");
                //TODO: [Network] Give item to player database
            }
        }

        private void HandleSettingButtonClicked()
        {
            Debug.Log("[MainMenu] Settings button clicked");
            // TODO: Open settings UI

            settingController.Init();
            settingController.OnSignOut += () =>
            {
                // HACK: Temporary used for signOut.
                NetworkAuth.Instance.SignOut();
                SceneManager.LoadScene(SceneList.Authentication.ToString());
            };
        }
    }
}
