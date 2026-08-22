using NaughtyAttributes;
using QuizGame.Item;
using QuizGame.Item.Interfaces;
using QuizGame.MyRoom.Decoration;
using UnityEngine;

namespace QuizGame.Store
{
    [CreateAssetMenu(fileName = "NewProductMetada", menuName = "QuizGame/ProductMetada", order = 0)]
    public class InGameProductMetadataSO : ScriptableObject, IInGameProductMetadata
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
    }
}
