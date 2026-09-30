using System.Threading.Tasks;
using QuizGame.Network;
using QuizGame.Network.FirestoreDataModels;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QuizGame.Ads;
using QuizGame.Character;
using QuizGame.Destination;
using QuizGame.Gameplay.Quiz;
using QuizGame.Gameplay.QuizManagement;
using QuizGame.Gameplay.UI;
using QuizGame.Item;
using QuizGame.Player;
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
        [Tooltip("Scene backdrop. Swapped to the selected destination's artwork when there is one.")]
        private SpriteRenderer backgroundRenderer;

        [SerializeField]
        [Tooltip("One entry per player index. Opponent avatars are spawned here; index 0 (the local player) is ignored.")]
        private Transform[] opponentPlaceHolders;

        // The multiplayer narrator slot lives under multiplayerScenario, which SetGameMode
        // disables in single player, so single player needs its own slot.
        [SerializeField]
        private Transform singlePlayerNpcPlaceHolder;

        [SerializeField]
        private int quizCount = 20;

        [SerializeField]
        private float timePerQuestion = 20f;

        public static string SinglePlayerMode = "Library";
        public static bool IsRankedMatch;
        [Tooltip("Grey-ball material quantity. Zero keeps rewards disabled until the economy is approved.")]
        [SerializeField] private int luckyDrawMaterialQuantity;
        private ItemWithQuantityPair pendingDraw;
        private int pendingPlace;
        private string pendingMatchId;
        private bool savingDraw;
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
            ApplyDestinationBackground();
            SpawnNPC();
            InitializeUI();
            InitializeQuizzes();
            StartCoroutine(StartQuestionSequence());
            SetGameMode(CurrentGameMode);
            ShowLocalPlayerName();
        }

        /// <summary>
        /// Keeps the backdrop on the destination that was drawn. Single player has no destination,
        /// so the scene's own artwork is left alone.
        /// </summary>
        private void ApplyDestinationBackground()
        {
            if (backgroundRenderer == null || SelectedDestinationInfo == null) return;

            var background = SelectedDestinationInfo.GetBackgroundSprite();
            if (background == null)
            {
                Debug.LogWarning($"[GameplayController] Destination '{SelectedDestinationInfo.GetID()}' has no background sprite; keeping the scene default.");
                return;
            }
            backgroundRenderer.sprite = background;
        }

        private async void ShowLocalPlayerName()
        {
            var profile = profileTask == null ? await PlayerDataManager.Instance.GetProfileData() : await profileTask;
            if (this == null || mainGameplayUI == null) return;

            var displayName = string.IsNullOrWhiteSpace(profile?.ProfileName) ? "Player" : profile.ProfileName;
            mainGameplayUI.SetPlayerName(LOCAL_PLAYER_INDEX, displayName);
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
                    ShowOpponents();
                    break;
            }
        }

        /// <summary>
        /// Draws the three opponents from <see cref="MultiplayerRoster"/> — the same roster the lobby
        /// showed — as player avatars in their slots, with their names on the HUD. Display only:
        /// MultiplayerResultReceiver still refuses to invent opponents for the result screen.
        /// </summary>
        private void ShowOpponents()
        {
            MultiplayerRoster.EnsurePlaceholders(CharacterResourceManager.Instance.GetAllResourcesID());
            var roster = MultiplayerRoster.Opponents;

            for (int i = 0; i < playerScores.Length; i++)
            {
                if (i == LOCAL_PLAYER_INDEX) continue;

                var rosterIndex = i > LOCAL_PLAYER_INDEX ? i - 1 : i;
                if (rosterIndex >= roster.Count) continue;

                var opponent = roster[rosterIndex];
                mainGameplayUI.SetPlayerName(i, opponent.DisplayName);
                mainGameplayUI.SetPlayerPoint(i, 0);
                SpawnOpponentAvatar(i, opponent.CharacterId);
            }
        }

        /// <summary>
        /// Opponents are players, so their slot shows a playable character rather than one of the
        /// NPC figures the placeholder art used.
        /// </summary>
        private void SpawnOpponentAvatar(int playerIndex, string characterId)
        {
            if (opponentPlaceHolders == null || playerIndex >= opponentPlaceHolders.Length) return;
            var slot = opponentPlaceHolders[playerIndex];
            if (slot == null || string.IsNullOrEmpty(characterId)) return;

            var info = CharacterResourceManager.Instance.GetResource(characterId);
            var prefab = info != null ? info.GetCharacterPrefab() : null;
            if (prefab == null)
            {
                Debug.LogWarning($"[GameplayController] Opponent character '{characterId}' has no prefab; slot {playerIndex} left empty.");
                return;
            }
            Instantiate(prefab, slot);
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
            currentQuizzes = new QuizData[CurrentGameMode == GameMode.Multiplayer ? 16 : quizCount];
            var filteredQuizzes = QuizCollections.GetAllQuizzes();

            if (SelectedQuizCategories != null && SelectedQuizCategories.Count > 0)
            {
                filteredQuizzes = filteredQuizzes
                    .FilterByCategories(SelectedQuizCategories.ToArray())
                    .ToList();
            }

            if (filteredQuizzes == null || filteredQuizzes.Count == 0)
            {
                Debug.LogError($"[GameplayController] Not enough quizzes in selected categories. Requested: {quizCount}, Available: {filteredQuizzes?.Count ?? 0}. Filling with random quizzes.");
                filteredQuizzes = QuizCollections.GetAllQuizzes();
            }

            if (CurrentGameMode == GameMode.SinglePlayer)
                filteredQuizzes = filteredQuizzes.Where(q => q.Type != QuizType.NumberGuessing).ToList();
            if (filteredQuizzes.Count == 0) { currentQuizzes = System.Array.Empty<QuizData>(); return; }

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
            if (CurrentGameMode == GameMode.Multiplayer && openingRewards && pendingDraw == null &&
                currentGameplayUI is GameRewardScreenUI && multiplayerResults.IsFinal) OpenMultiplayerRewards();
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
                EnsureStandInResults();
                OpenMultiplayerRewards();
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

        /// <summary>
        /// Closes the match locally when nothing has supplied a server snapshot, so the reward and
        /// lucky draw screens are reachable. Scores for the three opponents are invented, matching
        /// the roster the lobby and the match already showed; the local score is the real one.
        ///
        /// Delete this call once a room adapter calls SetFinalResults — SetStandInResults refuses
        /// to overwrite a real snapshot, so the two cannot fight.
        /// </summary>
        private void EnsureStandInResults()
        {
            if (multiplayerResults == null || multiplayerResults.IsFinal || localResult == null) return;

            MultiplayerRoster.EnsurePlaceholders(CharacterResourceManager.Instance.GetAllResourcesID());
            var roster = MultiplayerRoster.Opponents;
            if (roster.Count < MultiplayerRoster.OpponentCount) return;

            // Every score must differ: OpenMultiplayerRewards refuses to award a tied placement,
            // which would put the draw right back out of reach. Offsets are drawn without
            // replacement and zero is reserved for the player, so no two players can match.
            var offsets = new List<int> { -3, -2, -1, 1, 2, 3 };
            var players = new List<PlayerGameResultData>(MultiplayerRoster.OpponentCount + 1) { localResult.Copy() };
            for (int i = 0; i < MultiplayerRoster.OpponentCount; i++)
            {
                var pick = Random.Range(0, offsets.Count);
                var point = Mathf.Max(0, localResult.Point + offsets[pick]);
                offsets.RemoveAt(pick);
                players.Add(new PlayerGameResultData(roster[i].DisplayName, point)
                {
                    UserId = "standin-" + i,
                    CharacterId = roster[i].CharacterId
                });
            }

            // Mathf.Max can still collapse negatives onto 0 when the player scored very low, so
            // separate any duplicates upward rather than hand back a tie.
            var used = new HashSet<int>();
            foreach (var player in players.OrderByDescending(p => p.Point))
            {
                while (!used.Add(player.Point)) player.Point++;
            }

            var matchId = "standin-" + System.Guid.NewGuid().ToString("N");
            if (!multiplayerResults.SetStandInResults(matchId, players.ToArray()))
            {
                Debug.LogWarning("[GameplayController] Stand-in results were refused; rewards stay pending.");
            }
        }

        private void OpenMultiplayerRewards()
        {
            var screen = UIManager.Instance.Replace<GameRewardScreenUI>(ref currentGameplayUI);
            if (!IsRankedMatch)
            {
                screen.SetupMatch("Casual Match", "Casual matches do not award RP or items.", "Main Menu");
                screen.OnNextButtonClicked += ReturnToMainMenu;
                return;
            }
            var players = GetSortedPlayerResults();
            if (!multiplayerResults.IsFinal || !multiplayerResults.Results.Any(x => x.UserId == localResult.UserId))
            {
                screen.SetupMatch("Results pending", "Waiting for the final scores of all players.", "Main Menu");
                screen.OnNextButtonClicked += ReturnToMainMenu;
                return;
            }
            pendingPlace = System.Array.FindIndex(players, x => x.UserId == localResult.UserId) + 1;
            var finalLocalScore = players[pendingPlace - 1].Point;
            if (players.Count(x => x.Point == finalLocalScore) > 1)
            {
                screen.SetupMatch("Tied score", "Final placement is needed before awarding ranked rewards.", "Main Menu");
                screen.OnNextButtonClicked += ReturnToMainMenu;
                return;
            }
            pendingMatchId = multiplayerResults.MatchId;
            string missing = LuckyDrawRules.MissingPools(SelectedDestinationInfo);
            if (luckyDrawMaterialQuantity <= 0 || !string.IsNullOrEmpty(missing))
            {
                var reason = luckyDrawMaterialQuantity <= 0 ? "Reward amounts are awaiting configuration." : "Missing reward pools: " + missing;
                screen.SetupMatch("Place #" + pendingPlace, reason, "Main Menu");
                screen.OnNextButtonClicked += ReturnToMainMenu;
                Debug.LogWarning("[Rewards] " + reason + " Missing pools: " + missing);
                return;
            }
            int delta = LuckyDrawRules.RankingPoints(pendingPlace);
            screen.SetupMatch("Place #" + pendingPlace + "  RP " + delta.ToString("+0;-0;0"),
                "Draw your map reward. RP and item are saved together.", "Lucky Draw");
            screen.OnNextButtonClicked += ShowLuckyDraw;
        }

        private void ShowLuckyDraw()
        {
            var draw = UIManager.Instance.Replace<LuckyDrawUI>(ref currentGameplayUI);
            draw.SetupBonusMessage("Place #" + pendingPlace, LuckyDrawRules.OddsText(pendingPlace));
            draw.OnStartDrawReward += () =>
            {
                if (pendingDraw != null) return;
                int roll = Random.Range(0, 10000);
                var tier = LuckyDrawRules.RollTier(pendingPlace, roll);
                int itemRoll = Random.Range(0, LuckyDrawRules.Pool(SelectedDestinationInfo, tier).Length);
                pendingDraw = LuckyDrawRules.Choose(SelectedDestinationInfo, pendingPlace, roll, itemRoll, luckyDrawMaterialQuantity);
            };
            draw.OnEndDrawReward += SaveDraw;
        }

        private async void SaveDraw()
        {
            if (savingDraw || pendingDraw == null) return;
            savingDraw = true;
            var proposed = new MatchRewardReceipt { ItemId = pendingDraw.GetID(), Name = pendingDraw.GetName(),
                Type = pendingDraw.GetItemType().ToString(), Quantity = pendingDraw.GetQuantity(), Place = pendingPlace };
            var receipt = await PlayerDataManager.Instance.ClaimLuckyDraw(pendingMatchId, localResult.UserId, proposed);
            if (this == null) return;
            savingDraw = false;
            if (receipt == null)
            {
                var retry = UIManager.Instance.Replace<GameRewardScreenUI>(ref currentGameplayUI);
                retry.SetupMatch("Could not save", "Your draw is kept. Retry to claim the same reward.", "Retry");
                retry.OnNextButtonClicked += SaveDraw;
                return;
            }
            // A previous claim wins over a later random roll (retry/reopened match).
            var reward = new[] { ItemTier.SuperRare, ItemTier.Rare, ItemTier.Uncommon, ItemTier.Common, ItemTier.NoTier }
                .SelectMany(t => LuckyDrawRules.Pool(SelectedDestinationInfo, t))
                .FirstOrDefault(x => x.GetID() == receipt.ItemId && x.GetItemType().ToString() == receipt.Type);
            if (reward == null)
            {
                var saved = UIManager.Instance.Replace<GameRewardScreenUI>(ref currentGameplayUI);
                saved.SetupMatch("Reward saved", receipt.ItemId + " x" + receipt.Quantity, "Main Menu");
                saved.OnNextButtonClicked += ReturnToMainMenu;
                return;
            }
            var result = UIManager.Instance.Replace<LuckyDrawResultUI>(ref currentGameplayUI);
            result.Setup(new ItemWithQuantityPair(reward, receipt.Quantity));
            result.onAcceptButtonClicked += ReturnToMainMenu;
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
