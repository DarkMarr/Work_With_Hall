using System;
using QuizGame.Interfaces;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace QuizGame.Store.UI
{
    public class BundleVisualization : MonoBehaviour, IHasRectTransform
    {
        [SerializeField]
        private RectTransform rectTransform;

        [SerializeField]
        private BundleBanner bundleBanner;

        [SerializeField]
        private QuizGameIAPButton buyButton;

        [SerializeField]
        private TextMeshProUGUI buyButtonText;

        [SerializeField]
        private Button detailsButton;

        private void OnValidate()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void Setup(Product bundleProduct)
        {
            buyButton.Setup(bundleProduct);
            var priceLocalizedString = $"<size=36>{CurrencyLocaleHelper.FormatCurrencyForProduct(bundleProduct)}";
            buyButtonText.text = $"Buy now\n{priceLocalizedString}";

            var bundleBannerInfo = new BundleBanner.Info(
                bundleName: bundleProduct.metadata.localizedTitle,
                limitedTime: "Limited dd:hh:mm",
                bundleSprite: null // Placeholder for bundle sprite, can be set later
            );

            bundleBanner.Setup(bundleBannerInfo);

            detailsButton.onClick.AddListener(() =>
            {
                var bundleDetailsUI = UIManager.Instance.Create<BundleDetailsUI>();
                bundleDetailsUI.Init(
                        product: bundleProduct,
                        bundleBannerInfo: bundleBannerInfo,
                        onBackButtonClicked: () => bundleDetailsUI.Close());
            });
        }

        public RectTransform GetRectTransform() => rectTransform;
    }
}
