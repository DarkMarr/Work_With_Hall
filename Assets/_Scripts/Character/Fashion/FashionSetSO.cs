using System;
using System.Collections.Generic;
using NaughtyAttributes;
using QuizGame.Interfaces;
using QuizGame.Resources;
using UnityEngine;
using UnityEngine.Localization;

namespace QuizGame.Character.Fashion
{
    /// <summary>
    /// One wearable outfit, e.g. "Fashion_C_Baseball". Each set carries up to six pieces, one per
    /// decoration slot, and a character wears a set by taking every piece it has.
    ///
    /// The sprites live in <c>Assets/Art/Sprites/Fashion</c> as one PSB per set. Only the visible
    /// layer group is exported, so a PSB yields exactly the six slot sprites and nothing else —
    /// which is why a piece is identified by its sprite name rather than by a layer path.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFashionSet", menuName = "QuizGame/Character/Fashion Set", order = 2)]
    public class FashionSetSO : ScriptableObject, IHasID, IHasName
    {
        [SerializeField, ReadOnly]
        private string fashionSetID;

        [SerializeField, Tooltip("Set name without the rarity prefix, e.g. 'Baseball' for Fashion_C_Baseball.")]
        private string setName;

        [SerializeField]
        private FashionRarity rarity;

        [SerializeField]
        private LocalizedString localizedName;

        [SerializeField]
        private List<FashionPiece> pieces = new List<FashionPiece>();

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(fashionSetID))
            {
                fashionSetID = name;
            }
        }

        public string GetID() => fashionSetID;

        /// <summary>Falls back to the raw id while the string table has no entry yet.</summary>
        public string GetName() => localizedName.IsEmpty ? setName : localizedName.GetLocalizedString();

        public FashionRarity GetRarity() => rarity;
        public IReadOnlyList<FashionPiece> GetPieces() => pieces;

        /// <summary>Null when this set has nothing for that slot, which is normal.</summary>
        public Sprite GetPiece(CharacterPartType slot)
        {
            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i].Slot == slot) return pieces[i].Sprite;
            }
            return null;
        }
    }

    public enum FashionRarity
    {
        Common,
        Uncommon,
        Rare,
        SuperRare
    }

    [Serializable]
    public struct FashionPiece
    {
        [Tooltip("Which decoration slot this sprite fills.")]
        public CharacterPartType Slot;

        [ShowAssetPreview]
        public Sprite Sprite;
    }
}
