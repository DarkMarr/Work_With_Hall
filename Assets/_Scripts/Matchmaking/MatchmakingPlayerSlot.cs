using UnityEngine;

namespace QuizGame.Matchmaking
{
    public class MatchmakingPlayerSlot : MonoBehaviour
    {
        public enum PlayerSlotState
        {
            Loading,
            PlayerPresent
        }

        [SerializeField]
        private GameObject loadingVisual;

        [SerializeField]
        private GameObject playerVisual;

        private void Start()
        {
            SetPlayerSlotState(PlayerSlotState.Loading);
        }

        public void SetPlayerSlotState(PlayerSlotState state)
        {
            loadingVisual.SetActive(state == PlayerSlotState.Loading);
            playerVisual.SetActive(state == PlayerSlotState.PlayerPresent);
        }
    }
}
