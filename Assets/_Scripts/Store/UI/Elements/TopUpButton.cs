using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

namespace QuizGame.Store.UI
{
    public class TopUpButton : QuizGameIAPButton
    {
        [SerializeField]
        private TextMeshProUGUI currencyAmountText;

        [SerializeField]
        private TextMeshProUGUI priceText;

        public override void Setup(Product product)
        {
            base.Setup(product);
            var productPayout = IAPManager.Instance.GetProductPayout(product);
            currencyAmountText.text = productPayout.quantity.ToString();

            var priceLocalizedString = CurrencyLocaleHelper.FormatCurrencyForProduct(product);
            priceText.text = priceLocalizedString;
        }
    }
}
