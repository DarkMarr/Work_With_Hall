using System;
using TMPro;
using System.Threading.Tasks;
using QuizGame.Destination;
using QuizGame.Item.Interfaces;
using QuizGame.Item.UI;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.UI;
using QuizGame.Destination.UI;

namespace QuizGame.Matchmaking.UI
{
    public class MatchmakingUI : BaseUI
    {
        public event Action OnPreparingBackButtonClicked;
        public event Action OnMatchmakingBackButtonClicked;
        public event Action<int> OnReadyButtonClicked;

        [SerializeField]
        private TextMeshProUGUI matchMakingTypeText;

        [Header("Preparing State")]
        [SerializeField]
        private GameObject preparingContentContainer;

        [SerializeField]
        private Button readyButton;

        [SerializeField]
        private Button preparingStateBackButton;

        [SerializeField]
        private CarryOnItemSelection carryOnItemSelection;

        [SerializeField]
        private DestinationCategorySelectionScrollView destinationScrollView;

        [Header("Matchmaking State")]
        [SerializeField]
        private GameObject matchmakingContentContainer;

        [SerializeField]
        private GameObject matchmakingCountdownVisual;

        [SerializeField]
        private TextMeshProUGUI matchmakingCountdownText;

        [SerializeField]
        private Button matchmakingBackButton;

        [SerializeField]
        private GameObject nextDestinationPanel;

        [SerializeField]
        private PlayerSelectedMap[] playerSelectedMaps;

        void Start()
        {
            preparingStateBackButton.onClick.AddListener(() => OnPreparingBackButtonClicked?.Invoke());
            matchmakingBackButton.onClick.AddListener(() => OnMatchmakingBackButtonClicked?.Invoke());
            readyButton.onClick.AddListener(() => OnReadyButtonClicked?.Invoke(0));
            nextDestinationPanel.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matchmakingType"></param>
        /// <param name="availableDestinations"></param>
        /// <param name="avaliableItems">Avaliable items in player's inventory.</param>
        /// <param name="currentCarryOnItems">Carrying item in the slot.</param>
        /// <param name="onCarryOnItemChange"></param>
        /// <param name="onReadyButtonClicked"></param>
        public void Init(
                        MatchmakingType matchmakingType,
                        IDestinationInfo[] availableDestinations,
                        IQuantifiableItem[] availableItems)
        {
            switch (matchmakingType)
            {
                case MatchmakingType.Ranking:
                    matchMakingTypeText.text = "Ranking Match"; //TODO: Replace with localization
                    break;

                case MatchmakingType.Casual:
                    matchMakingTypeText.text = "Casual Match"; //TODO: Replace with localization
                    break;
            }
            destinationScrollView.Init(availableDestinations);
            carryOnItemSelection.Init(availableItems);
        }

        public void SwitchUIState(MatchmakingState state)
        {
            preparingContentContainer.SetActive(state == MatchmakingState.Preparing);
            matchmakingContentContainer.SetActive(state == MatchmakingState.Waiting);
        }

        public IQuantifiableItem[] GetSelectedCarryOnItems()
        {
            return carryOnItemSelection.GetSelectedCarryOnItems();
        }

        public void SetMatchmakingDescriptionText(string text)
        {
            matchmakingCountdownText.text = text;
        }

        public void SetPlayerSelectedMapSprite(int playerIndex, Sprite mapSprite)
        {
            if (playerIndex < 0 || playerIndex >= playerSelectedMaps.Length)
            {
                Debug.LogError($"[MatchmakingUI] Invalid player index: {playerIndex}");
                return;
            }
            playerSelectedMaps[playerIndex].SetMapSprite(mapSprite);
        }

        public void ClearAllPlayerSelectedMaps()
        {
            for (int i = 0; i < playerSelectedMaps.Length; i++)
            {
                playerSelectedMaps[i].SetMapSprite(null);
            }
        }

        public async void StartCountdown(int millisecond)
        {
            var currentTime = (float)millisecond;

            while (currentTime > 0)
            {
                var second = currentTime / 1000;
                SetMatchmakingDescriptionText($"{Mathf.Ceil(second)}");
                await Task.Delay(1000);
                currentTime -= 1000;
            }
        }

        public void ShowNextDestinationPanel(bool show)
        {
            nextDestinationPanel.SetActive(show);
        }

        public void HighlightPlayerSelectedMapIndex(int playerIndex)
        {
            for (int i = 0; i < playerSelectedMaps.Length; i++)
            {
                SetPlayerMapShading(i, i != playerIndex);
            }
        }

        public void SetPlayerMapShading(int playerIndex, bool enabled)
        {
            if (playerIndex < 0 || playerIndex >= playerSelectedMaps.Length)
            {
                Debug.LogError($"[MatchmakingUI] Invalid player index: {playerIndex}");
                return;
            }
            playerSelectedMaps[playerIndex].SetShading(enabled);
        }

        public void SetBackButtonActive(bool isActive)
        {
            matchmakingBackButton.gameObject.SetActive(isActive);
        }

        public void SetCountdownVisualActive(bool isActive)
        {
            matchmakingCountdownVisual.SetActive(isActive);
        }
    }
}