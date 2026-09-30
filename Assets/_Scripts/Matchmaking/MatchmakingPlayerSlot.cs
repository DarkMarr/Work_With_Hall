using QuizGame.Character;
using TMPro;
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

        [SerializeField, Tooltip("Default character art under playerVisual. Its bounds define where and how big a real avatar is drawn.")]
        private GameObject placeholderAvatar;

        [Header("Status Bubble")]
        [SerializeField, Tooltip("Text shown while this slot is still waiting for a player.")]
        private TMP_Text waitingText;

        [SerializeField, Tooltip("Player name shown in the ready bubble.")]
        private TMP_Text playerNameText;

        [SerializeField, Tooltip("Player rank shown under the name in the ready bubble.")]
        private TMP_Text playerRankText;

        private GameObject avatarInstance;

        private void Awake()
        {
            ResolveStatusBubbleReferences();
        }

        private void Start()
        {
            SetPlayerSlotState(PlayerSlotState.Loading);
        }

        public void SetPlayerSlotState(PlayerSlotState state)
        {
            if (loadingVisual != null)
                loadingVisual.SetActive(state == PlayerSlotState.Loading);
            if (playerVisual != null)
                playerVisual.SetActive(state == PlayerSlotState.PlayerPresent);
        }

        public void SetPlayerInfo(string playerName, string rankName)
        {
            ResolveStatusBubbleReferences();

            if (playerNameText != null)
                playerNameText.text = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName;
            if (playerRankText != null)
                playerRankText.text = string.IsNullOrWhiteSpace(rankName) ? "Unranked" : rankName;
        }

        private void ResolveStatusBubbleReferences()
        {
            if (waitingText == null && loadingVisual != null)
                waitingText = loadingVisual.transform
                    .Find("Waiting-Canvas/TextBox/Waiting-Text (TMP)")
                    ?.GetComponent<TMP_Text>();

            if (playerVisual == null) return;

            if (playerNameText == null)
                playerNameText = playerVisual.transform
                    .Find("Ready-Canvas/TextBox/name-Text (TMP)")
                    ?.GetComponent<TMP_Text>();
            if (playerRankText == null)
                playerRankText = playerVisual.transform
                    .Find("Ready-Canvas/TextBox/rank-Text (TMP)")
                    ?.GetComponent<TMP_Text>();

            if (waitingText != null && string.IsNullOrWhiteSpace(waitingText.text))
                waitingText.text = "Waiting...";
        }

        /// <summary>
        /// Shows the given character prefab in place of the placeholder, fitted to the placeholder's height
        /// and standing on the same spot. Pass null to go back to the placeholder.
        /// </summary>
        public void SetAvatar(GameObject avatarPrefab)
        {
            if (avatarInstance != null)
            {
                Destroy(avatarInstance);
                avatarInstance = null;
            }

            if (placeholderAvatar == null)
            {
                Debug.LogWarning($"[MatchmakingPlayerSlot] '{name}' has no placeholder avatar assigned.");
                return;
            }

            placeholderAvatar.SetActive(avatarPrefab == null);
            if (avatarPrefab == null) return;

            var space = playerVisual.transform;
            if (!CharacterSpriteBounds.TryGetBounds(placeholderAvatar.transform, space, out var target))
            {
                Debug.LogWarning($"[MatchmakingPlayerSlot] Placeholder on '{name}' has no sprites to measure.");
                placeholderAvatar.SetActive(true);
                return;
            }

            avatarInstance = Instantiate(avatarPrefab, space);
            avatarInstance.transform.localPosition = Vector3.zero;
            avatarInstance.transform.localRotation = Quaternion.identity;
            avatarInstance.transform.localScale = Vector3.one;

            if (!CharacterSpriteBounds.TryGetBounds(avatarInstance.transform, space, out var source))
            {
                Debug.LogWarning($"[MatchmakingPlayerSlot] Avatar '{avatarPrefab.name}' has no sprites to show.");
                return;
            }

            // Match the placeholder's height, centre it horizontally and put its feet where the placeholder's are.
            float scale = target.height / source.height;
            avatarInstance.transform.localScale = new Vector3(scale, scale, 1f);
            avatarInstance.transform.localPosition = new Vector3(
                target.center.x - source.center.x * scale,
                target.yMin - source.yMin * scale,
                0f);
        }
    }
}
