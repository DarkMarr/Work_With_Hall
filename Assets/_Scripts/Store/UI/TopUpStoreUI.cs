using System;
using System.Collections.Generic;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace QuizGame.Store.UI
{
    public class TopUpStoreUI : BaseUI
    {
        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private Transform topUpButtonContainer;

        [SerializeField]
        private TopUpButton topUpButtonPrefab;

        public void Init(List<Product> products, Action onCloseButtonClicked)
        {
            closeButton.onClick.AddListener(() => onCloseButtonClicked?.Invoke());
            foreach (var product in products)
            {
                var topUpButton = Instantiate(topUpButtonPrefab, topUpButtonContainer);
                topUpButton.Setup(product);
            }
        }
    }
}
