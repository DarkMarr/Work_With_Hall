using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QuizGame.Ads;
using QuizGame.Destination;
using QuizGame.Gameplay.Quiz;
using QuizGame.Gameplay.QuizManagement;
using QuizGame.Gameplay.UI;
using QuizGame.Item;
using QuizGame.Scene;
using QuizGame.Store;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuizGame.Gameplay
{
    public class GameplayController : MonoBehaviour
    {
        public enum GameMode
        {
            SinglePlayer,
            Multiplayer
        }

        //TODO: this is temp static for testing purpose, making some data collection class is better.
        public static List<QuizCategory> SelectedQuizCategories = new List<QuizCategory>();
        public static IDestinationInfo SelectedDestinationInfo; //TODO: making some data collection class is better.
        public static GameMode CurrentGameMode; //TODO: making some data collection class is better.

        [SerializeField]
        private QuizController quizController;

        [SerializeField]
        private GameObject singlePlayerScenario;

        [SerializeField]
        private GameObject multiplayerScenario;

        [SerializeField]
        private Transform npcPlaceHolder;

        [SerializeField]
        private int quizCount = 20;

        [SerializeField]
        private float timePerQuestion = 20f;

        private GameplayUI mainGameplayUI;
        private BaseUI currentGameplayUI;
        private QuizData[] currentQuizzes;
        private int[] playerScores = new int[4]; //TODO: [Network] get real player scores
        private int localPlayerCorrectAnswerCount = 0;
        private float quizTimer = 0f;
        private bool isTimerRunning = false;

        private const int CORRECT_ANSWER_POINTS = 100;
        private const int INCORRECT_ANSWER_POINTS = 0;
        private const int LOCAL_PLAYER_INDEX = 0;
        private const float DELAY_BETWEEN_QUESTIONS = 2f;
        private const int ADS_REWARD_MULTIPLIER = 2;

        void Start()
        {
            var npc = SelectedDestinationInfo != null ? SelectedDestinationInfo.GetNPCPrefab() : null;
            if (npc != null)
            {
                Instantiate(npc, npcPlaceHolder);
            }

            InitializeUI();
            InitializeQuizzes();
            StartCoroutine(StartQuestionSequence());
            SetGameMode(CurrentGameMode);
        }

        void Update()
        {
            HandleDebugInput();
            UpdateQuizTimer();
        }

        public void SetGameMode(GameMode gameMode)
        {
            singlePlayerScenario.SetActive(GameMode.SinglePlayer == gameMode);
            multiplayerScenario.SetActive(GameMode.Multiplayer == gameMode);

            switch (gameMode)
            {
                case GameMode.SinglePlayer:
                    mainGameplayUI.SetEnablePlayerUIs(0, 3);
                    break;

                case GameMode.Multiplayer:
                    mainGameplayUI.SetEnablePlayerUIs(0, 1, 2, 3);
                    break;
            }
        }

        private void InitializeUI()
        {
            UIManager.Instance.CloseAll();
            mainGameplayUI = UIManager.Instance.Create<GameplayUI>();
            currentGameplayUI = mainGameplayUI;
            quizController.SetQuizContainer(mainGameplayUI.GetQuizContainer());
        }

        private void InitializeQuizzes()
        {
            currentQuizzes = new QuizData[quizCount];
            var filteredQuizzes = QuizCollections.GetAllQuizzes();

            if (SelectedQuizCategories != null && SelectedQuizCategories.Count > 0)
            {
                filteredQuizzes = filteredQuizzes
                    .FilterByCategories(SelectedQuizCategories.ToArray())
                    .ToList();
            }

            if (filteredQuizzes == null || filteredQuizzes.Count <= quizCount)
            {
                Debug.LogError($"[GameplayController] Not enough quizzes in selected categories. Requested: {quizCount}, Available: {filteredQuizzes.Count}. Filling with random quizzes.");
                filteredQuizzes = QuizCollections.GetAllQuizzes();
            }

            for (int i = 0; i < currentQuizzes.Length; i++)
            {
                currentQuizzes[i] = filteredQuizzes.GetRandomQuiz();
            }
        }

        private IEnumerator StartQuestionSequence()
        {
            PrepareGameStart();

            for (int i = 0; i < currentQuizzes.Length; i++)
            {
                yield return PresentQuiz(i);
                yield return new WaitForSeconds(DELAY_BETWEEN_QUESTIONS);
            }

            Debug.Log("[GameplayController] End of all quizzes.");
            EndGame();
        }

        private void PrepareGameStart()
        {
            mainGameplayUI.ClearAllPlayerPoint();
            mainGameplayUI.SetCorrectAnswerCountText(0);
            quizController.onSubmitAnswerButtonClicked += () => isTimerRunning = false;
        }

        private IEnumerator PresentQuiz(int quizIndex)
        {
            var quizData = currentQuizzes[quizIndex];

            StartQuizTimer();
            UpdateQuizUI(quizIndex, quizData);
            Debug.Log($"[GameplayController] StartQuestionSequence - Quiz Number: {quizIndex + 1}/{currentQuizzes.Length}: {quizData.GetQuestionLocalize()} (Type: {quizData.Type})");

            quizController.CloseCurrentQuiz();
            yield return quizController.StartQuizCoroutine(quizData, OnQuizAnswered);
        }

        private void StartQuizTimer()
        {
            isTimerRunning = true;
            quizTimer = timePerQuestion;
        }

        private void UpdateQuizUI(int quizIndex, QuizData quizData)
        {
            mainGameplayUI.SetCurrentAnswerCountText(quizIndex + 1, currentQuizzes.Length);
            mainGameplayUI.SetNarratorText(quizData.GetQuestionLocalize());
        }

        private void OnQuizAnswered(bool isCorrect)
        {
            if (isCorrect)
            {
                HandleCorrectAnswer();
            }
            else
            {
                HandleIncorrectAnswer();
            }
        }

        private void HandleCorrectAnswer()
        {
            Debug.Log($"[GameplayController] OnQuizAnswered - Correct answer! +{CORRECT_ANSWER_POINTS} points");
            mainGameplayUI.SetNarratorText($"Correct! +{CORRECT_ANSWER_POINTS} points");

            //TODO: This is single player test, change to real multiplayer 
            playerScores[LOCAL_PLAYER_INDEX] += CORRECT_ANSWER_POINTS;
            mainGameplayUI.SetPlayerPoint(LOCAL_PLAYER_INDEX, playerScores[LOCAL_PLAYER_INDEX]);
            mainGameplayUI.SetCorrectAnswerCountText(++localPlayerCorrectAnswerCount);
        }

        private void HandleIncorrectAnswer()
        {
            Debug.Log($"[GameplayController] OnQuizAnswered - Wrong answer! +{INCORRECT_ANSWER_POINTS} points");
            mainGameplayUI.SetNarratorText($"Wrong! +{INCORRECT_ANSWER_POINTS} points");
        }

        private void UpdateQuizTimer()
        {
            if (!isTimerRunning) return;

            quizTimer -= Time.deltaTime;

            if (quizTimer <= 0f)
            {
                HandleTimerExpired();
            }
            else
            {
                UpdateTimerDisplay();
            }
        }

        private void HandleTimerExpired()
        {
            quizTimer = 0f;
            isTimerRunning = false;
            quizController.DisableCurrentQuizInteraction();
            mainGameplayUI.SetNarratorText($"Time's up! +{INCORRECT_ANSWER_POINTS} points");
            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            mainGameplayUI.SetTimerPercentage(quizTimer / timePerQuestion);
        }

        public void EndGame()
        {
            quizController.DisableCurrentQuizInteraction();
            ShowGameResults();
        }

        private void ShowGameResults()
        {
            var playerResultDatas = GetSortedPlayerResults();
            var resultScreenUI = UIManager.Instance.Replace<GameResultScreenUI>(ref currentGameplayUI);
            resultScreenUI.Init(playerResultDatas);
            resultScreenUI.onRewardButtonClicked += OpenRewardUI;
        }

        private PlayerGameResultData[] GetSortedPlayerResults()
        {
            var playerResultDatas = PlayerGameResultData.FromJson(PlayerGameResultData.GetJsonTempData());
            return playerResultDatas.OrderByDescending(x => x.Point).ToArray();
        }

        public void OpenRewardUI()
        {
            var rewardScreenUI = UIManager.Instance.Replace<GameRewardScreenUI>(ref currentGameplayUI);
            var rewardItems = GetRewardItems();

            SetupRewardScreen(rewardScreenUI, rewardItems);
        }

        private ItemWithQuantityPair[] GetRewardItems()
        {
            //TODO: [Network] load real rewards
            return ItemHelper.GetItemsWithQuantityFromDataJson(GetRewardsDataTempJson()).ToArray();
        }

        private void SetupRewardScreen(GameRewardScreenUI rewardScreenUI, ItemWithQuantityPair[] rewardItems)
        {
            rewardScreenUI.SetupRewards(rewardItems);
            rewardScreenUI.SetRankingPointVisualize(125, 3450, 150); //TODO: [Network] load real rank points
            rewardScreenUI.OnAdsButtonClicked += () => HandleRewardScreenAdsButtonClicked(rewardScreenUI, rewardItems);
            rewardScreenUI.OnNextButtonClicked += ShowLuckyDraw;
        }

        public void HandleRewardScreenAdsButtonClicked(GameRewardScreenUI rewardScreenUI, ItemWithQuantityPair[] rewardItems)
        {
            //TODO: Watch ads before add reward
            if (AdsManager.Instance.IsRewardedAdAvailable())
            {
                AdsManager.Instance.ShowRewardedAd(new AdsManager.RewardedAdShowCallbacks()
                {
                    OnAdRewarded = (adInfo, rewardInfo) =>
                    {
                        Debug.Log("[GameplayController] Rewarded Ad watched completely. Granting double rewards.");
                        MultiplyRewardQuantities(rewardItems, ADS_REWARD_MULTIPLIER);
                        rewardScreenUI.SetupRewards(rewardItems);
                        rewardScreenUI.SetAdsEnable(false);
                    },
                    OnAdClosed = (adInfo) =>
                    {
                        Debug.Log("[GameplayController] Rewarded Ad closed.");
                    }
                });
            }
            else
            {
                //TODO: Show some UI to inform user that ads is not available
                Debug.Log("[GameplayController] Rewarded Ad not available.");
            }
        }

        private void MultiplyRewardQuantities(ItemWithQuantityPair[] rewardItems, int multiplier)
        {
            foreach (var reward in rewardItems)
            {
                reward.SetQuantity(reward.GetQuantity() * multiplier);
            }
        }

        public void ShowLuckyDraw()
        {
            var luckyDrawUI = UIManager.Instance.Replace<LuckyDrawUI>(ref currentGameplayUI);
            luckyDrawUI.SetupBonusMessage("1st place\nBonus Rate!", "Unique x1.5\nRare x1.75");
            luckyDrawUI.OnEndDrawReward += ShowDrawResult;
        }

        public void ShowDrawResult()
        {
            var luckyDrawResultUI = UIManager.Instance.Replace<LuckyDrawResultUI>(ref currentGameplayUI);

            if (TryGetLuckyDrawItem(out var drawItem))
            {
                luckyDrawResultUI.Setup(drawItem);
            }
            else
            {
                Debug.LogError("No item found for lucky draw");
            }

            luckyDrawResultUI.onAcceptButtonClicked += ReturnToMainMenu;
        }

        private bool TryGetLuckyDrawItem(out ItemWithQuantityPair drawItem)
        {
            //TODO: [Network] Get real result from network
            return ItemHelper.TryGetItemFromDataJson(GetLuckyRewardDataTempJson(), out drawItem);
        }

        private void ReturnToMainMenu()
        {
            SceneManager.LoadScene(SceneList.MainMenu.ToString());
        }

        private void HandleDebugInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EndGame();
            }
        }

        public string GetLuckyRewardDataTempJson() => @"
            {
                ""item_id"": ""33304"",
                ""item_type"": 2,
                ""quantity"": 20
            }
        ";

        public string GetRewardsDataTempJson() => @"[
            {
                ""item_id"": ""Coin"",
                ""item_type"": 4,
                ""quantity"": 5000
            },
            {
                ""item_id"": ""50003"",
                ""item_type"": 3,
                ""quantity"": 10
            },
            {
                ""item_id"": ""50004"",
                ""item_type"": 3,
                ""quantity"": 10
            }
        ]";
    }
}