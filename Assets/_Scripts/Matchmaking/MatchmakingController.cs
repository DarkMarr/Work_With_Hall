using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QuizGame.Destination;
using QuizGame.Destination.UI;
using QuizGame.Item;
using QuizGame.Item.Interfaces;
using QuizGame.Matchmaking.UI;
using QuizGame.Scene;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;

namespace QuizGame.Matchmaking
{
    public class MatchmakingController : MonoBehaviour
    {
        public MatchmakingState CurrentState { get; private set; }
        public Action<bool> OnMatchmakingCompleted; // Parameter indicates whether matchmaking was successful or failed (e.g., due to timeout or cancellation)

        public event Action OnExitMatchmaking;
        public IDestinationInfo SelectedDestinationInfo { get; private set; }

        [SerializeField]
        private MatchmakingWorldSpaceVisual matchmakingWorldSpaceVisual;

        private BaseUI previousUI;
        private MatchmakingUI matchmakingUI;
        private UIManager uiManager;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Dictionary<int, IDestinationInfo> playerSelectedDestinations = new Dictionary<int, IDestinationInfo>();

        private const int MatchStartDelayMS = 20000; // TODO: Replace with server-side delay

        public void Start()
        {
            SetMatchmakingState(MatchmakingState.None);
            CloseMatchmakingVisuals();
        }

        private void OnDestroy()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }

        private void CloseMatchmakingVisuals()
        {
            matchmakingWorldSpaceVisual.Close();
        }

        public void InjectUIManager(UIManager uiManager)
        {
            this.uiManager = uiManager;
        }

        /// <summary>
        /// Opens the matchmaking sequence UI and initializes player selections.
        /// </summary>
        public void OpenMatchmakingSequence(
            MatchmakingType matchmakingType,
            ItemWithQuantityPair[] availableItems,
            IDestinationInfo[] availableDestinations,
            ref BaseUI currentActiveUI,
            Action<bool> onMatchmakingCompleted = null)
        {
            playerSelectedDestinations.Clear();
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = new CancellationTokenSource();

            previousUI = currentActiveUI;
            matchmakingUI = uiManager.Replace<MatchmakingUI>(ref currentActiveUI);

            matchmakingUI.Init(
                matchmakingType: matchmakingType,
                availableDestinations: availableDestinations,
                availableItems: availableItems
            );

            matchmakingUI.OnReadyButtonClicked += destinationSelectedIndex => OnReadyForMatchClicked(matchmakingType, destinationSelectedIndex);
            matchmakingUI.OnPreparingBackButtonClicked += ExitMatchMaking;
            matchmakingUI.OnMatchmakingBackButtonClicked += ExitMatchMaking;

            SetMatchmakingState(MatchmakingState.Preparing);
            OnMatchmakingCompleted = onMatchmakingCompleted;
        }

        private void OnReadyForMatchClicked(MatchmakingType matchmakingType, int destinationSelectedIndex)
        {
            var selectedCarryOnItems = matchmakingUI.GetSelectedCarryOnItems();

            foreach (var item in selectedCarryOnItems)
            {
                Debug.Log($"[Matchmaking] Player selected carry-on item: {item?.GetID() ?? "None"}");
            }

            StartMatchmakingSequence(matchmakingType, selectedCarryOnItems, destinationSelectedIndex);
        }

        public void SetMatchmakingState(MatchmakingState newState)
        {
            matchmakingUI?.SwitchUIState(newState);
            CurrentState = newState;
        }

        public void ExitMatchMaking()
        {
            cancellationTokenSource.Cancel();

            SetMatchmakingState(MatchmakingState.None);
            CloseMatchmakingVisuals();

            if (previousUI != null)
            {
                previousUI.Show();
                matchmakingUI.Close();
            }
            else
            {
                matchmakingUI?.Close();
            }

            OnExitMatchmaking?.Invoke();
        }

        public void StartMatchmakingSequence(MatchmakingType matchmakingType, IItem[] carryOnItems, int selectedDestinationIndex)
        {
            Debug.Log($"[Matchmaking] Starting matchmaking sequence...");
            Debug.Log($"[Matchmaking] Matchmaking Type: {matchmakingType}");

            var selectedDestination = DestinationResourceManager.Instance.GetResourceAtIndex(selectedDestinationIndex);
            Debug.Log($"[Matchmaking] Player selected destination: {selectedDestination.GetName()}"); //TODO: [Network] Save player selected destination

            foreach (var item in carryOnItems)
            {
                if (item != null)
                    Debug.Log($"[Matchmaking] Carry-on item: {item.GetID()}"); //TODO: [Network] Save Carry-on item player take to server
            }

            SetMatchmakingState(MatchmakingState.Matchmaking);

            _ = StartMatchmaking(
                onLobbyFound: HandleLobbyFound,
                onMatchMakingComplete: async destinationPlayerWinningVoteIndex =>
                {
                    await OnMatchmakingComplete(destinationPlayerWinningVoteIndex);
                }
            );
        }

        private void HandleLobbyFound(bool isComplete)
        {
            if (isComplete)
            {
                SetMatchmakingState(MatchmakingState.Waiting);
            }
            else
            {
                HandleLobbyFailed();
            }
        }

        private void HandleLobbyFailed()
        {
            Debug.LogWarning("[Matchmaking] Lobby not found.");
            OnMatchmakingCompleted?.Invoke(false);
            // TODO: [Network] Show error message or fallback
        }

        public async Task OnMatchmakingComplete(int destinationPlayerWinningVoteIndex)
        {
            SelectedDestinationInfo = playerSelectedDestinations[destinationPlayerWinningVoteIndex];

            matchmakingUI.SetCountdownVisualActive(false);
            matchmakingUI.SetBackButtonActive(false);
            matchmakingUI.ShowNextDestinationPanel(true);
            matchmakingUI.HighlightPlayerSelectedMapIndex(destinationPlayerWinningVoteIndex);

            OnMatchmakingCompleted?.Invoke(true);
            
            await Task.Delay(4000);

            matchmakingUI.Close();
            StartGame();
        }

        private void StartGame()
        {
            SceneManager.LoadScene(SceneList.Gameplay.ToString());
        }

        private async Task StartMatchmaking(Action<bool> onLobbyFound, Action<int> onMatchMakingComplete)
        {
            Debug.Log($"[Matchmaking] Starting matchmaking...");

            // TODO: [Network] Replace with real server request
            Debug.Log($"[Matchmaking] Lobby found.");
            var isLobbyFound = true;
            onLobbyFound?.Invoke(isLobbyFound);

            if (!isLobbyFound)
            {
                Debug.LogError("[Matchmaking] Failed to find lobby.");
                return;
            }

            Debug.Log("[Matchmaking] Waiting for other players...");
            matchmakingWorldSpaceVisual.Open();
            matchmakingWorldSpaceVisual.ResetMatchmakingVisuals();
            matchmakingUI.ClearAllPlayerSelectedMaps();

            await Task.Yield();

            var maxPlayer = 4;

            Debug.Log("[Matchmaking] Simulating player joins...");

            matchmakingWorldSpaceVisual.ShowJoinedPlayer(0); // Show self as joined
            var selectedDestination = DestinationResourceManager.Instance.GetRandomResource(); // TODO: [Network] Get the selected map from player vote
            playerSelectedDestinations.Add(0, selectedDestination);
            matchmakingUI.SetPlayerSelectedMapSprite(0, selectedDestination.GetSprite());
            matchmakingUI.SwitchUIState(MatchmakingState.Waiting);
            matchmakingUI.StartCountdown(MatchStartDelayMS);

            // Simulate waiting for players
            //TODO: [Network] Show joined players from server
            var stopwatch = Stopwatch.StartNew();
            try
            {
                for (int i = 1; i < maxPlayer; i++)
                {
                    if (cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    await Task.Delay(4000 + (i * 1000), cancellationTokenSource.Token);
                    PlayerJoinTheLobby(i);
                }
                var timeLeftTillStartGame = MatchStartDelayMS - (int)stopwatch.ElapsedMilliseconds;
                await Task.Delay(timeLeftTillStartGame, cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                Debug.Log("[Matchmaking] Matchmaking wait cancelled by user.");
            }

            if (cancellationTokenSource.Token.IsCancellationRequested)
            {
                return;
            }

            var destinationPlayerWinningVoteIndex = Random.Range(0, 4); // TODO: [Network] Get the **winning vote map index** from the player votes.
            Debug.Log($"[Matchmaking] Match ready. We select map that vote by player index: {destinationPlayerWinningVoteIndex}");

            onMatchMakingComplete?.Invoke(destinationPlayerWinningVoteIndex);
        }

        private void PlayerJoinTheLobby(int playerIndex)
        {
            matchmakingWorldSpaceVisual.ShowJoinedPlayer(playerIndex);
            var selectedDestination = DestinationResourceManager.Instance.GetRandomResource(); // TODO: [Network] Get the selected map from player vote
            playerSelectedDestinations.Add(playerIndex, selectedDestination);
            matchmakingUI.SetPlayerSelectedMapSprite(playerIndex, selectedDestination.GetSprite());
            Debug.Log($"[MatchmakingWorldSpaceVisual] Player {playerIndex + 1} has joined the matchmaking.");
        }
    }
}