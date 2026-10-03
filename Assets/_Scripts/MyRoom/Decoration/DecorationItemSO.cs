using QuizGame.Item;
using QuizGame.Item.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Localization;

namespace QuizGame.MyRoom.Decoration
{
    [CreateAssetMenu(fileName = "NewDecoration", menuName = "QuizGame/Item/Decoration", order = 1)]
    public class DecorationItemSO : BaseItemSO, IDecorationItem
    {
        protected override ItemType ItemType => ItemType.Decoration;
        protected override ItemTier ItemTier => decorationTier;

        [SerializeField]
        private DecorationType decorationType;

        [SerializeField]
        [Tooltip("The decoration as it stands in the room. BaseItemSO's sprite is the inventory " +
                 "icon, which is framed and tinted by rarity and has no business on a shelf, so " +
                 "a decoration with nothing here simply is not drawn in the room.")]
        private Sprite roomSprite;

        [SerializeField]
        private ItemTier decorationTier;

        [SerializeField]
        private LocalizedString localizedName;

        [SerializeField]
        private LocalizedString localizedDescription;

        [SerializeField]
        private LocalizedString localizedSubDescription;

        [SerializeField]
        private ItemSOWithQuantityPair[] recycledItems;

        [SerializeField]
        [FormerlySerializedAs("fuseRequirementItems")]
        private ItemSOWithQuantityPair[] craftRequirementItems;

        public string GetDescription() => localizedDescription.GetLocalizedString();
        public override string GetName() => localizedName.GetLocalizedString();
        public DecorationType GetDecorationType() => decorationType;
        /// <summary>
        /// Null until the decoration has been drawn for the room. The icon is deliberately not used
        /// as a stand-in: it carries a rarity frame, and a framed picture standing on the floor
        /// reads as a finished thing that is wrong rather than as art that is missing.
        /// </summary>
        public Sprite GetRoomSprite() => roomSprite;
        public string GetSubDescription() => localizedSubDescription.GetLocalizedString();
        public IQuantifiableItem[] GetRecycledItems() => recycledItems;
        public IQuantifiableItem[] GetCraftRequirementItems() => craftRequirementItems;
        public IItem GetCraftResult() => this;
    }
}
