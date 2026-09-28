using System.Threading.Tasks;
using QuizGame.Network;
using QuizGame.Network.FirestoreDataModels;
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

        // The multiplayer narrator slot lives under multiplayerScenario, which SetGameMode
        // disables in single player, so single player needs its own slot.
        [SerializeField]
        private Transform singlePlayerNpcPlaceHolder;

        [SerializeField]
        private int quizCount = 20;

        [SerializeField]
        private float timePerQuestion = 20f;

        public static string SinglePlayerMode = "Library";
        [SerializeField] private MultiplayerResultReceiver multiplayerResults;
        private Task<ProfileData> profileTask;
        private PlayerGameResultData localResult;
        private GameResultScreenUI resultUI;
        private bool gameEnded;
        private bool openingRewards;
        private bool answerRecorded;
        private string runId = System.Guid.NewGuid().ToString("N");
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
            profileTask = PlayerDataManager.Instance.GetProfileData();
            if (multiplayerResults == null)
            {
                var receiverObject = new GameObject("MultiplayerResults");
                receiverObject.transform.SetParent(transform, false);
                multiplayerResults = receiverObject.AddComponent<MultiplayerResultReceiver>();
            }
            multiplayerResults.ResultsChanged += RefreshMultiplayerResults;
            SpawnNPC();
            InitializeUI();
            InitializeQuizzes();
            StartCoroutine(StartQuestionSequence());
            SetGameMode(CurrentGameMode);
        }

        /// <summary>
        /// Spawns an NPC in the gameplay scene.
        /// In multiplayer, uses the destination's NPC. In single player, falls back to a random NPC from resources.
        /// </summary>
        private void SpawnNPC()
        {
            GameObject npcPrefab = null;

            // Multiplayer: use destination's NPC if available.
            if (SelectedDestinationInfo != null)
            {
                npcPrefab = SelectedDestinationInfo.GetNPCPrefab();
            }

            // Single player fallback: pick a random NPC from the NPC resource manager.
            if (npcPrefab == null && CurrentGameMode == GameMode.SinglePlayer)
            {
                var npcManager = NpcResourceManager.Instance;
                if (npcManager != null && npcManager.Count() > 0)
                {
                    var randomNpc = npcManager.GetRandomResource();
                    npcPrefab = randomNpc?.GetNpcPrefab();
                    Debug.Log($"[GameplayController] Single player: spawned random NPC '{randomNpc?.GetName()}'.");
                }
                else
                {
                    Debug.LogWarning("[GameplayController] No NPC resources found in Resources/NPCs.");
                }
            }

            if (npcPrefab != null)
            {
                var placeHolder = CurrentGameMode == GameMode.SinglePlayer && singlePlayerNpcPlaceHolder != null
                    ? singlePlayerNpcPlaceHolder
                    : npcPlaceHolder;
                Instantiate(npcPrefab, placeHolder);
            }
        }

        void Update()
        {
#if UNITY_EDITOR
            HandleDebugInput();
#endif
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
            answerRecorded = false;
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
            if (gameEnded || answerRecorded || quizTimer <= 0) return;
            answerRecorded = true;
            isTimerRunning = false;
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
            int points = CalculateAnswerScore(quizTimer, timePerQuestion);
            Debug.Log($"[GameplayController] OnQuizAnswered - Correct answer! +{points} points");
            mainGameplayUI.SetNarratorText($"Correct! +{points} points");

            //TODO: This is single player test, change to real multiplayer 
            playerScores[LOCAL_PLAYER_INDEX] += points;
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

        // GAME DESIGN DOC 02.xlsx, quiz!A10:A12. Rounded to the nearest integer.
        public static int CalculateAnswerScore(float timeLeft, float maxTime)
            => maxTime <= 0 || timeLeft <= 0 ? 0 : Mathf.RoundToInt(100 + Mathf.Clamp01(timeLeft / maxTime) * 100);

        public async void EndGame()
        {
            if (gameEnded) return;
            gameEnded = true;
            isTimerRunning = false;
            StopAllCoroutines();
            quizController.DisableCurrentQuizInteraction();
            quizController.CloseCurrentQuiz();
            var profile = profileTask == null ? await PlayerDataManager.Instance.GetProfileData() : await profileTask;
            if (this == null) return;
            var user = FirebaseConnection.Auth.CurrentUser;
            localResult = new PlayerGameResultData(profile?.ProfileName ?? "Player", playerScores[LOCAL_PLAYER_INDEX]) {
                UserId = user?.UserId, CharacterId = profile?.CharacterId,
                EquippedItems = profile?.EquippedItems, IsLocalPlayer = true
            };
            if (string.IsNullOrEmpty(localResult.CharacterId))
                localResult.CharacterId = QuizGame.Character.PlayerCharacterManager.Instance.SelectedCharacterId;
            ShowGameResults();
        }

        private void ShowGameResults()
        {
            resultUI = UIManager.Instance.Replace<GameResultScreenUI>(ref currentGameplayUI);
            resultUI.Init(GetSortedPlayerResults());
            resultUI.onRewardButtonClicked += OpenRewardUI;
        }

        private PlayerGameResultData[] GetSortedPlayerResults()
        {
            if (CurrentGameMode == GameMode.SinglePlayer) return new[] { localResult };
            var results = multiplayerResults.Results.ToList();
            // Show the actual local score while waiting for a complete network snapshot.
            if (localResult != null && !results.Any(p => p.UserId == localResult.UserId)) results.Add(localResult.Copy());
            foreach (var result in results) result.IsLocalPlayer = result.UserId == localResult?.UserId;
            return results.OrderByDescending(p => p.Point).ThenBy(p => p.UserId, System.StringComparer.Ordinal).Take(4).ToArray();
        }

        private void RefreshMultiplayerResults()
        {
            if (CurrentGameMode == GameMode.Multiplayer && resultUI != null) resultUI.SetResults(GetSortedPlayerResults());
        }

        private void OnDestroy()
        {
            if (multiplayerResults != null) multiplayerResults.ResultsChanged -= RefreshMultiplayerResults;
        }

        public async void OpenRewardUI()
        {
            if (openingRewards || localResult == null) return;
            openingRewards = true;
            if (CurrentGameMode == GameMode.Multiplayer)
            {
                // Network reward settlement has not been connected; don't award sample items/RP.
                ReturnToMainMenu();
                return;
            }
            var rewardUI = UIManager.Instance.Replace<GameRewardScreenUI>(ref currentGameplayUI);
            rewardUI.SetupSinglePlayer(localResult.Point, null, "Saving score...");
            var best = await PlayerDataManager.Instance.RecordSinglePlayerResult(runId, SinglePlayerMode, localResult.Point);
            if (this == null || rewardUI == null) return;
            if (best.HasValue)
            {
                rewardUI.SetupSinglePlayer(localResult.Point, best, "Item rewards are not configured yet.");
                rewardUI.OnNextButtonClicked += ReturnToMainMenu;
            }
            else
            {
                rewardUI.SetupSinglePlayer(localResult.Point, null, "Could not save. Tap Retry.", "Retry");
                rewardUI.OnNextButtonClicked += () => { openingRewards = false; OpenRewardUI(); };
            }
        }

        private void ReturnToMainMenu()
        {
            SceneManager.LoadScene(SceneList.MainMenu.ToString());
        }

#if UNITY_EDITOR
        private void HandleDebugInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EndGame();
            }
        }
#endif

    }
}
