using QuizGame.Item;
using UnityEngine;
using UnityEngine.Localization;

namespace QuizGame.Material
{
    [CreateAssetMenu(fileName = "NewMaterial", menuName = "QuizGame/Item/Material", order = 1)]
    public class MaterialInfoSO : BaseItemSO, IMaterial
    {
        [SerializeField]
        private LocalizedString localizedName;

        public override string GetName() => localizedName.GetLocalizedString();
        protected override ItemType ItemType => ItemType.Material;
        protected override ItemTier ItemTier => ItemTier.NoTier;
    }
}
