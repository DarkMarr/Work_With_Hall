using QuizGame.Interfaces;
using UnityEngine;
using UnityEngine.Localization;

namespace QuizGame.Item
{
    [CreateAssetMenu(fileName = "newEquipment", menuName = "QuizGame/Item/Equipment", order = 1)]
    public class EquipmentItemSO : BaseItemSO, IHasDescription
    {
        [SerializeField]
        private LocalizedString localizedName;

        [SerializeField]
        private LocalizedString localizedDescription;

        [SerializeField]
        private LocalizedString localizedSubDescription;

        protected override ItemType ItemType => ItemType.Equipment;

        [SerializeField] private ItemTier equipmentTier = ItemTier.NoTier;
        protected override ItemTier ItemTier => equipmentTier;

        public override string GetName() => localizedName.GetLocalizedString();

        public string GetDescription() => localizedDescription.GetLocalizedString();

        public string GetSubDescription() => localizedSubDescription.GetLocalizedString();
    }
}
