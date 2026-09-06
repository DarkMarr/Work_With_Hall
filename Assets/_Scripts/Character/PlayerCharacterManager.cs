using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuizGame.Network;
using QuizGame.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuizGame.Character
{
    /// <summary>
    /// Central singleton that manages the player character across scenes.
    /// Auto-spawns the player at PlayerSpawnPoints, loads/saves outfit from Firestore,
    /// and provides API for character selection and cosmetic equip/unequip.
    /// </summary>
    public class PlayerCharacterManager : MonoSingleton<PlayerCharacterManager>
    {
        public event Action<PlayerCharacter> OnPlayerSpawned;
        public event Action OnOutfitChanged;

        private PlayerCharacter currentPlayer;
        private PlayerOutfit currentOutfit;

        /// <summary>
        /// The currently spawned player character instance (null if not spawned yet).
        /// </summary>
        public PlayerCharacter CurrentPlayer => currentPlayer;

        /// <summary>
        /// The currently selected character ID.
        /// </summary>
        public string SelectedCharacterId => currentOutfit?.CharacterId;

        protected override void Awake()
        {
            base.Awake();
            currentOutfit = new PlayerOutfit();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var spawnPoints = UnityEngine.Object.FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.InstanceID);
            if (spawnPoints == null || spawnPoints.Length == 0) return;

            // Find the first active spawn point.
            PlayerSpawnPoint targetSpawn = null;
            foreach (var sp in spawnPoints)
            {
                if (sp.SpawnOnSceneLoad && sp.gameObject.activeInHierarchy)
                {
                    targetSpawn = sp;
                    break;
                }
            }
            if (targetSpawn == null) return;

            // Load outfit from database then spawn.
            _ = LoadOutfitAndSpawn(targetSpawn);
        }

        private async Task LoadOutfitAndSpawn(PlayerSpawnPoint spawnPoint)
        {
            // Load profile data to get saved character and equipped items.
            var profileData = await PlayerDataManager.Instance.GetProfileData();
            if (profileData != null)
            {
                currentOutfit.CharacterId = profileData.CharacterId ?? string.Empty;
                currentOutfit.EquippedItems = profileData.EquippedItems ?? new Dictionary<string, string>();
            }

            SpawnPlayerAtPoint(spawnPoint);
        }

        private void SpawnPlayerAtPoint(PlayerSpawnPoint spawnPoint)
        {
            // Destroy existing player if any.
            if (currentPlayer != null)
            {
                Destroy(currentPlayer.gameObject);
                currentPlayer = null;
            }

            // If no character selected, use the first available character from resources.
            if (string.IsNullOrEmpty(currentOutfit.CharacterId))
            {
                var allCharacters = CharacterResourceManager.Instance.GetAllResources();
                if (allCharacters != null && allCharacters.Length > 0)
                {
                    currentOutfit.CharacterId = allCharacters[0].GetID();
                }
                else
                {
                    Debug.LogWarning("[PlayerCharacterManager] No character SOs found in Resources/Characters.");
                    return;
                }
            }

            var characterInfo = CharacterResourceManager.Instance.GetResource(currentOutfit.CharacterId);
            if (characterInfo == null)
            {
                Debug.LogError($"[PlayerCharacterManager] Character '{currentOutfit.CharacterId}' not found.");
                return;
            }

            var prefab = characterInfo.GetCharacterPrefab();
            if (prefab == null)
            {
                Debug.LogError($"[PlayerCharacterManager] Character '{currentOutfit.CharacterId}' has no prefab assigned.");
                return;
            }

            var instance = Instantiate(prefab, spawnPoint.transform.position, Quaternion.identity);
            if (!spawnPoint.FaceRight)
            {
                instance.transform.localScale = new Vector3(-1f, 1f, 1f);
            }

            currentPlayer = instance.GetComponent<PlayerCharacter>();
            if (currentPlayer == null)
            {
                currentPlayer = instance.AddComponent<PlayerCharacter>();
            }
            currentPlayer.SetCharacterId(currentOutfit.CharacterId);

            // Apply equipped cosmetics.
            ApplyCurrentOutfit();

            OnPlayerSpawned?.Invoke(currentPlayer);
            Debug.Log($"[PlayerCharacterManager] Player spawned as '{currentOutfit.CharacterId}' at '{spawnPoint.name}'.");
        }

        /// <summary>
        /// Converts the saved string-based equipped items dictionary to CharacterPartType-based labels
        /// and applies them to the current player.
        /// </summary>
        private void ApplyCurrentOutfit()
        {
            if (currentPlayer == null || currentOutfit == null) return;

            var equippedLabels = new Dictionary<CharacterPartType, string>();
            if (currentOutfit.EquippedItems != null)
            {
                foreach (var kvp in currentOutfit.EquippedItems)
                {
                    // kvp.Key = cosmetic item ID, look up the cosmetic SO to get partType and spriteLabel.
                    // For now, we store the mapping as partType -> spriteLabel directly in the equipped items.
                    // The key format is the CharacterPartType enum name, value is the sprite label.
                    if (Enum.TryParse<CharacterPartType>(kvp.Key, out var partType))
                    {
                        equippedLabels[partType] = kvp.Value;
                    }
                }
            }

            currentPlayer.ApplyOutfit(equippedLabels);
        }

        #region Public API

        /// <summary>
        /// Changes the player's character. Saves the selection to Firestore and respawns.
        /// </summary>
        public async Task SetSelectedCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return;

            currentOutfit.CharacterId = characterId;
            currentOutfit.EquippedItems = new Dictionary<string, string>(); // Clear cosmetics on character change.

            await PlayerDataManager.Instance.UpdateSelectedCharacter(characterId);
            await PlayerDataManager.Instance.UpdateEquippedItems(currentOutfit.EquippedItems);

            // Respawn the player with the new character.
            var spawnPoint = FindActiveSpawnPoint();
            if (spawnPoint != null)
            {
                SpawnPlayerAtPoint(spawnPoint);
            }

            OnOutfitChanged?.Invoke();
        }

        /// <summary>
        /// Equips a cosmetic item on the player. Saves to Firestore.
        /// </summary>
        public async Task EquipCosmetic(CharacterCosmeticItemSO cosmeticItem)
        {
            if (cosmeticItem == null || currentPlayer == null) return;

            // Check character restriction.
            if (cosmeticItem.IsRestrictedToCharacter(currentOutfit.CharacterId))
            {
                Debug.LogWarning($"[PlayerCharacterManager] Cosmetic '{cosmeticItem.GetID()}' is restricted to character '{cosmeticItem.GetRestrictedCharacterId()}'.");
                return;
            }

            var partType = cosmeticItem.GetPartType();
            var spriteLabel = cosmeticItem.GetSpriteLabel();

            // Validate the label exists in the current character's sprite library.
            if (!currentPlayer.SpriteMixer.HasLabel(partType, spriteLabel))
            {
                Debug.LogWarning($"[PlayerCharacterManager] Label '{spriteLabel}' not found for part type '{partType}' on character '{currentOutfit.CharacterId}'.");
                return;
            }

            // Update local state.
            currentOutfit.EquippedItems[partType.ToString()] = spriteLabel;

            // Apply immediately.
            currentPlayer.ApplyOutfit(GetEquippedLabelsDictionary());

            // Save to Firestore.
            await PlayerDataManager.Instance.UpdateEquippedItems(currentOutfit.EquippedItems);
            await PlayerDataManager.Instance.SetInventoryItemEquipped(cosmeticItem.GetID(), true);

            OnOutfitChanged?.Invoke();
        }

        /// <summary>
        /// Unequips the cosmetic from the given part type. Saves to Firestore.
        /// </summary>
        public async Task UnequipPart(CharacterPartType partType)
        {
            if (currentPlayer == null) return;

            var partKey = partType.ToString();
            if (currentOutfit.EquippedItems.ContainsKey(partKey))
            {
                currentOutfit.EquippedItems.Remove(partKey);
            }

            // Re-apply outfit (will reset the part to its initial label).
            currentPlayer.ApplyOutfit(GetEquippedLabelsDictionary());

            await PlayerDataManager.Instance.UpdateEquippedItems(currentOutfit.EquippedItems);

            OnOutfitChanged?.Invoke();
        }

        /// <summary>
        /// Reloads the outfit from Firestore and re-applies it.
        /// </summary>
        public async Task RefreshPlayerOutfit()
        {
            var profileData = await PlayerDataManager.Instance.GetProfileData();
            if (profileData != null)
            {
                currentOutfit.CharacterId = profileData.CharacterId ?? string.Empty;
                currentOutfit.EquippedItems = profileData.EquippedItems ?? new Dictionary<string, string>();
            }

            if (currentPlayer != null)
            {
                currentPlayer.ApplyOutfit(GetEquippedLabelsDictionary());
            }

            OnOutfitChanged?.Invoke();
        }

        #endregion

        #region Helpers

        private Dictionary<CharacterPartType, string> GetEquippedLabelsDictionary()
        {
            var dict = new Dictionary<CharacterPartType, string>();
            if (currentOutfit?.EquippedItems == null) return dict;

            foreach (var kvp in currentOutfit.EquippedItems)
            {
                if (Enum.TryParse<CharacterPartType>(kvp.Key, out var partType))
                {
                    dict[partType] = kvp.Value;
                }
            }
            return dict;
        }

        private PlayerSpawnPoint FindActiveSpawnPoint()
        {
            var spawnPoints = UnityEngine.Object.FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.InstanceID);
            foreach (var sp in spawnPoints)
            {
                if (sp.SpawnOnSceneLoad && sp.gameObject.activeInHierarchy)
                {
                    return sp;
                }
            }
            return null;
        }

        #endregion
    }
}
