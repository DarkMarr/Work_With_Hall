using System;
using System.Collections.Generic;
using QuizGame.Store.UI;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace QuizGame.Store
{
    public class BundleStoreUI : BaseUI
    {
        [SerializeField]
        private Button backButton;

        [SerializeField]
        private BundleListScrollView bundleListScrollView;

        private List<Product> bundleProducts;

        public void Init(List<Product> bundleProducts, Action onBackButtonClicked)
        {
            backButton.onClick.AddListener(() => onBackButtonClicked?.Invoke());

            this.bundleProducts = bundleProducts;
            bundleListScrollView.Init(
                dataAmount: bundleProducts.Count,
                onElementDataUpdate: OnBundleElementDataUpdate);
        }

        private void OnBundleElementDataUpdate(int dataIndex, BundleVisualization element)
        {
            element.Setup(bundleProducts[dataIndex]);
        }
    }
}

