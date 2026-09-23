using System.Collections.Generic;
using UnityEngine;

namespace QuizGame.Character
{
    /// <summary>
    /// Runtime component attached to the player character instance.
    /// Wraps CharacterSpriteMixer and applies the player's outfit (cosmetics) to it.
    /// </summary>
    public class PlayerCharacter : MonoBehaviour
    {
        private CharacterSpriteMixer spriteMixer;
        private Dictionary<CharacterPartType, string> initialLabels;

        public CharacterSpriteMixer SpriteMixer => spriteMixer;
        public string CurrentCharacterId { get; private set; }

        void Awake()
        {
            spriteMixer = GetComponent<CharacterSpriteMixer>();
            if (spriteMixer == null)
            {
                spriteMixer = GetComponentInChildren<CharacterSpriteMixer>();
            }
            CaptureInitialLabels();
        }

        private void CaptureInitialLabels()
        {
            initialLabels = new Dictionary<CharacterPartType, string>();
            if (spriteMixer == null) return;

            foreach (var category in spriteMixer.GetCategories())
            {
                if (category?.Resolver != null)
                {
                    initialLabels[category.PartType] = category.Resolver.GetLabel();
                }
            }
        }

        public void SetCharacterId(string characterId)
        {
            CurrentCharacterId = characterId;
        }

        /// <summary>
        /// Applies the equipped cosmetics to the character's sprite mixer.
        /// Parts not in the dictionary are reset to their initial labels.
        /// </summary>
        public void ApplyOutfit(Dictionary<CharacterPartType, string> equippedLabels)
        {
            if (spriteMixer == null)
            {
                // Whole-art avatars (e.g. UC_rabbit_*) have no mixer; only complain if cosmetics were requested.
                if (equippedLabels != null && equippedLabels.Count > 0)
                {
                    Debug.LogWarning("[PlayerCharacter] No CharacterSpriteMixer found; cosmetics cannot be applied.");
                }
                return;
            }

            // Reset all parts to initial labels first.
            foreach (var kvp in initialLabels)
            {
                spriteMixer.SetPartLabel(kvp.Key, kvp.Value);
            }

            // Apply equipped cosmetics (only if the label exists in the library).
            if (equippedLabels == null) return;

            foreach (var kvp in equippedLabels)
            {
                if (spriteMixer.HasLabel(kvp.Key, kvp.Value))
                {
                    spriteMixer.SetPartLabel(kvp.Key, kvp.Value);
                }
                else
                {
                    Debug.LogWarning($"[PlayerCharacter] Label '{kvp.Value}' not found for part type '{kvp.Key}'. Skipping.");
                }
            }
        }

        /// <summary>
        /// Clears all equipped cosmetics, reverting to initial appearance.
        /// </summary>
        public void ClearOutfit()
        {
            ApplyOutfit(null);
        }
    }
}
