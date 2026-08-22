using QuizGame.Interfaces;
using UnityEngine;
using UnityEngine.Localization;

namespace QuizGame.Item
{
    [CreateAssetMenu(fileName = "newCarryOnItem", menuName = "QuizGame/Item/CarryOnItem", order = 1)]
    //TODO: May link with item behaviour model, such as another SO for behaviour 
    public class CarryOnItemSO : BaseItemSO, IHasDescription
    {
        [SerializeField]
        private LocalizedString localizedName;

        [SerializeField]
        private LocalizedString localizedDescription;

        [SerializeField]
        private LocalizedString localizedSubDescription;

        protected override ItemType ItemType => ItemType.Consumable;

        protected override ItemTier ItemTier => ItemTier.NoTier;

        public override string GetName() => localizedName.GetLocalizedString();

        public string GetDescription() => localizedDescription.GetLocalizedString();

        public string GetSubDescription() => localizedSubDescription.GetLocalizedString();
    }
}
