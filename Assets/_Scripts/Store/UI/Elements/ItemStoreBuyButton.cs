using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Store
{
    [RequireComponent(typeof(Button))]
    public class ItemStoreBuyButton : MonoBehaviour
    {
        public event Action OnPurchaseButtonClicked;

        [SerializeField]
        private Button purchaseButton;

        [SerializeField]
        private Image currencyImage;

        [SerializeField]
        private TextMeshProUGUI priceText;

        private void Awake()
        {
            purchaseButton.onClick.AddListener(() => OnPurchaseButtonClicked?.Invoke());
        }

        private void OnValidate()
        {
            if (purchaseButton == null)
            {
                purchaseButton = GetComponent<Button>();
            }
        }

        public void Setup(IInGameProductMetadata product)
        {
            if (product == null)
            {
                currencyImage.sprite = null;
                priceText.text = $"No Product";
                return;
            }
            currencyImage.sprite = product.GetPurchasedCurrency().GetSprite();
            priceText.text = $"Purchase\n{product.GetPrice()}";
        }
    }
}
