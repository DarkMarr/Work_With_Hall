using System;
using Newtonsoft.Json;
using QuizGame.Currency;
using QuizGame.Interfaces;
using QuizGame.Item;
using QuizGame.Material;
using QuizGame.MyRoom.Decoration;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace QuizGame.Store.UI
{
    public class BundleDetailsUI : BaseUI
    {
        [SerializeField]
        private BundleBanner bundleBanner;

        [SerializeField]
        private TextMeshProUGUI bundleDescriptionText;

        [SerializeField]
        private Button backButton;

        [SerializeField]
        private QuizGameIAPButton buyButton;

        [SerializeField]
        private ImageWithTextVisualization ItemInBundleVisualization;

        [SerializeField]
        private Transform itemVisualizationContainer;

        private Product product;

        public void Init(Product product, BundleBanner.Info bundleBannerInfo, Action onBackButtonClicked)
        {
            this.product = product;
            bundleDescriptionText.text = product.metadata.localizedDescription;
            bundleBanner.Setup(bundleBannerInfo);
            backButton.onClick.AddListener(() => onBackButtonClicked?.Invoke());
            buyButton.Setup(product);

            var items = product.GetItemsWithQuantityFromPayoutData();
            foreach (var item in items)
            {
                if (item != null)
                {
                    var itemVisualization = Instantiate(ItemInBundleVisualization, itemVisualizationContainer);
                    itemVisualization.Setup(item.GetSprite(), $"x{item.GetQuantity()}");
                }
            }
        }
    }
}
