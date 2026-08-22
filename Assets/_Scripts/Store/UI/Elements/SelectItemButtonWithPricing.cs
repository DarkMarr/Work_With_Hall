using System;
using QuizGame.Item.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Store
{
    public class SelectItemButtonWithPricing : SelectItemButton
    {
        [SerializeField]
        private Image currencyImage;

        [SerializeField]
        private TextMeshProUGUI priceText;

        public void SetupCurrency(Sprite currencySprite, int price)
        {
            currencyImage.enabled = currencySprite != null;
            currencyImage.sprite = currencySprite;
            priceText.text = price > 0 ? price.ToString() : "";
        }
    }
}
