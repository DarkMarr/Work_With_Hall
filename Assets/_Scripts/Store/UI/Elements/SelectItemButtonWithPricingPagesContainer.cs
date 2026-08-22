using System.Collections.Generic;
using System.Linq;
using QuizGame.Interfaces;
using QuizGame.Item.UI;
using UnityEngine;

namespace QuizGame.Store
{
    public class SelectItemButtonWithPricingPagesContainer : SelectItemButtonPagesContainer
    {
        protected override void OnOpenPage(List<SelectItemButton> buttons, IHasSprite[] sprites)
        {
            if (buttons.Count <= 0) return;

            if (buttons[0] is SelectItemButtonWithPricing)
            {
                var pricingButtons = buttons.Cast<SelectItemButtonWithPricing>().ToArray();
                var products = sprites.Cast<IInGameProductMetadata>().ToArray();
                for (int i = 0; i < CountSpawnedObject(); i++)
                {
                    var button = pricingButtons[i];
                    if (i < products.Length && products[i] != null)
                    {
                        var product = products[i];
                        button.SetSprite(product.GetSprite());
                        button.SetupCurrency(product.GetPurchasedCurrency().GetSprite(), product.GetPrice());
                    }
                    else
                    {
                        button.SetSprite(null);
                        button.SetupCurrency(null, -1);
                    }
                }
            }

        }
    }
}
