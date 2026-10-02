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

        public override string GetName()
        {
            if (localizedName != null && !localizedName.IsEmpty) return localizedName.GetLocalizedString();
            // Generated outfit rewards have no translation keys yet. Never let an empty
            // table reference abort reward presentation (or prevent the Accept listener binding).
            // "Fashion" is still accepted because the art, and so the generated ids, kept the older
            // word; the double underscore separates the set from the slot, as in
            // Outfit_C_Baseball__HeadDecoration, and reads as a space.
            var parts = (GetID() ?? "").Split(new[] { '_' }, 3);
            var fallback = parts.Length == 3 && (parts[0] == "Outfit" || parts[0] == "Fashion")
                ? parts[2].Replace("__", " ")
                : name;
            return System.Text.RegularExpressions.Regex.Replace(fallback, "([a-z])([A-Z])", "$1 $2");
        }

        public string GetDescription() => localizedDescription == null || localizedDescription.IsEmpty
            ? "" : localizedDescription.GetLocalizedString();

        public string GetSubDescription() => localizedSubDescription == null || localizedSubDescription.IsEmpty
            ? "" : localizedSubDescription.GetLocalizedString();
    }
}
