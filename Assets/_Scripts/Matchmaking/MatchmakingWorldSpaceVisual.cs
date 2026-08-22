using UnityEngine;

namespace QuizGame.Matchmaking
{
    public class MatchmakingWorldSpaceVisual : MonoBehaviour
    {
        [SerializeField]
        private MatchmakingPlayerSlot[] playerSlots;

        public void ShowJoinedPlayer(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= playerSlots.Length)
            {
                Debug.LogError($"[MatchmakingWorldSpaceVisual] Invalid player index: {playerIndex}");
                return;
            }
            playerSlots[playerIndex].SetPlayerSlotState(MatchmakingPlayerSlot.PlayerSlotState.PlayerPresent);
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void ResetMatchmakingVisuals()
        {
            for (int i = 0; i < playerSlots.Length; i++)
            {
                playerSlots[i].SetPlayerSlotState(MatchmakingPlayerSlot.PlayerSlotState.Loading);
            }
        }
    }
}
