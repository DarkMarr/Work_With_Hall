using NaughtyAttributes;
using QuizGame.Interfaces;
using QuizGame.Item;
using QuizGame.Item.Interfaces;
using QuizGame.MyRoom.Decoration;
using UnityEngine;

namespace QuizGame.Store
{
    [CreateAssetMenu(fileName = "NewProductMetada", menuName = "QuizGame/ProductMetada", order = 0)]
    public class InGameProductMetadataSO : ScriptableObject, IInGameProductMetadata, IHasDescription
    {
        [SerializeField, ReadOnly]
        private string productID;

        [SerializeField]
        private BaseItemSO product;

        [SerializeField]
        private FilterType filterType;

        [SerializeField]
        private CurrencyInfoSO purchasedCurrency;

        [SerializeField]
        private int price;

        private void OnValidate()
        {
            productID = name;
            if (purchasedCurrency != null)
            {
                if (purchasedCurrency.GetCurrencyType() == CurrencyType.Coin)
                {
                    filterType |= FilterType.Coin;
                    filterType &= ~FilterType.Gem;
                }
                else if (purchasedCurrency.GetCurrencyType() == CurrencyType.Gem)
                {
                    filterType |= FilterType.Gem;
                    filterType &= ~FilterType.Coin;
                }
            }
        }

        public IItem GetItemProduct() => product;
        public ICurrencyInfo GetPurchasedCurrency() => purchasedCurrency;
        public int GetPrice() => price;
        public string GetID() => productID;
        public ItemType GetItemType() => product.GetItemType();
        public string GetName() => product.GetName();
        public Sprite GetSprite() => product.GetSprite();
        public ItemTier GetItemTier() => product.GetItemTier();
        public FilterType GetFilterType() => filterType;

        /// <summary>
        /// Passes the item's own words through, the way GetName and GetSprite already do.
        ///
        /// A shelf hands the selection panel the product, not the item inside it, and the panel
        /// only fills its two description lines when what it is given implements this interface.
        /// Without it no product in any shop ever showed a description: the panel kept whatever
        /// placeholder text the prefab was authored with, which read as if the copy was missing.
        /// </summary>
        public string GetDescription() => product is IHasDescription described ? described.GetDescription() : "";

        public string GetSubDescription() => product is IHasDescription described ? described.GetSubDescription() : "";
    }
}
