using QuizGame.Item;
using UnityEngine;

namespace QuizGame.Store
{
    [CreateAssetMenu(fileName = "NewCurrency", menuName = "QuizGame/Item/Currency", order = 1)]
    public class CurrencyInfoSO : BaseItemSO, ICurrencyInfo
    {
        protected override ItemType ItemType => ItemType.Currency;
        protected override ItemTier ItemTier => ItemTier.NoTier;

        [SerializeField]
        private CurrencyType currencyType;

        public CurrencyType GetCurrencyType() => currencyType;
    }
}
