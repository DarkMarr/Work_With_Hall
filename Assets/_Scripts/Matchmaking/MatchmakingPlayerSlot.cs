using QuizGame.Character;
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

        private GameObject avatarInstance;

        private void Start()
        {
            SetPlayerSlotState(PlayerSlotState.Loading);
        }

        public void SetPlayerSlotState(PlayerSlotState state)
        {
            loadingVisual.SetActive(state == PlayerSlotState.Loading);
            playerVisual.SetActive(state == PlayerSlotState.PlayerPresent);
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
