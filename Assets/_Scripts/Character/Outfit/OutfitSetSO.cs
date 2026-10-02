using System;
using System.Collections.Generic;
using NaughtyAttributes;
using QuizGame.Interfaces;
using QuizGame.Resources;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Localization;

namespace QuizGame.Character.Outfit
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
    public class OutfitSetSO : ScriptableObject, IHasID, IHasName
    {
        [SerializeField, ReadOnly]
        [FormerlySerializedAs("fashionSetID")]
        private string outfitSetID;

        [SerializeField, Tooltip("Set name without the rarity prefix, e.g. 'Baseball' for Fashion_C_Baseball.")]
        private string setName;

        [SerializeField]
        private OutfitRarity rarity;

        [SerializeField]
        private LocalizedString localizedName;

        [SerializeField]
        private List<OutfitPiece> pieces = new List<OutfitPiece>();

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(outfitSetID))
            {
                outfitSetID = name;
            }
        }

        public string GetID() => outfitSetID;

        /// <summary>Falls back to the raw id while the string table has no entry yet.</summary>
        public string GetName() => localizedName.IsEmpty ? setName : localizedName.GetLocalizedString();

        public OutfitRarity GetRarity() => rarity;
        public IReadOnlyList<OutfitPiece> GetPieces() => pieces;

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

    public enum OutfitRarity
    {
        Common,
        Uncommon,
        Rare,
        SuperRare
    }

    [Serializable]
    public struct OutfitPiece
    {
        [Tooltip("Which decoration slot this sprite fills.")]
        public CharacterPartType Slot;

        [ShowAssetPreview]
        public Sprite Sprite;
    }
}
