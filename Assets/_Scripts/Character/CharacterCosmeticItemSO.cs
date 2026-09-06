using QuizGame.Item;
using UnityEngine;

namespace QuizGame.Character
{
    /// <summary>
    /// A fashion/cosmetic item that can be equipped on a character part.
    /// Defines which part type it applies to and which sprite label to use in the sprite library.
    /// Optionally restricted to a specific character (empty = works on any character).
    /// </summary>
    [CreateAssetMenu(fileName = "NewCosmeticItem", menuName = "QuizGame/Character/Cosmetic Item", order = 1)]
    public class CharacterCosmeticItemSO : EquipmentItemSO
    {
        [SerializeField]
        private CharacterPartType partType;

        [SerializeField]
        private string spriteLabel;

        [Tooltip("Leave empty to allow this cosmetic on any character. Set to a character ID to restrict.")]
        [SerializeField]
        private string restrictedCharacterId;

        public CharacterPartType GetPartType() => partType;
        public string GetSpriteLabel() => spriteLabel;
        public string GetRestrictedCharacterId() => restrictedCharacterId;
        public bool IsRestrictedToCharacter(string characterId) => !string.IsNullOrEmpty(restrictedCharacterId) && restrictedCharacterId != characterId;
    }
}
