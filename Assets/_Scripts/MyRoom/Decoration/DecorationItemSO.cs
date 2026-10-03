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
                 "icon, which is framed and tinted by rarity and would look wrong on a shelf. " +
                 "Leave this empty and the icon is used, which is what a wallpaper wants.")]
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
        public Sprite GetRoomSprite() => roomSprite != null ? roomSprite : GetSprite();
        public string GetSubDescription() => localizedSubDescription.GetLocalizedString();
        public IQuantifiableItem[] GetRecycledItems() => recycledItems;
        public IQuantifiableItem[] GetCraftRequirementItems() => craftRequirementItems;
        public IItem GetCraftResult() => this;
    }
}
