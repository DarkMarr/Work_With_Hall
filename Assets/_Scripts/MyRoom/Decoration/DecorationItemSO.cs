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
        public string GetSubDescription() => localizedSubDescription.GetLocalizedString();
        public IQuantifiableItem[] GetRecycledItems() => recycledItems;
        public IQuantifiableItem[] GetCraftRequirementItems() => craftRequirementItems;
        public IItem GetCraftResult() => this;
    }
}
