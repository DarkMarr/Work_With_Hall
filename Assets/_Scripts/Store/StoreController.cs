using System;
using System.Collections.Generic;
using QuizGame.Store.UI;
using QuizGame.UI;
using UnityEngine;
using QuizGame.Network;

namespace QuizGame.Store
{
    [Serializable]
    public class StoreController
    {
        public event Action OnMainStoreBackButtonClicked;

        /// <summary>Raised after a purchase succeeds, so a balance on screen can be refreshed.</summary>
        public event Action OnCurrencyChanged;
        
        private BaseUI currentUI;

        private void Back()
        {
            currentUI?.Close();
            OnMainStoreBackButtonClicked?.Invoke();
        }

        public void OpenMainStore()
        {
            var mainStoreUI = UIManager.Instance.Replace<MainStoreUI>(ref currentUI);
            mainStoreUI.Init(
                onBundleStoreButtonClicked: OpenBundleStore,
                onAvatarStoreButtonClicked: OpenAvatarStore,
                onRoomStoreButtonClicked: OpenRoomStore,
                onItemStoreButtonClicked: OpenItemStore,
                onTopUpButtonClicked: OpenTopUpStore,
                onBackButtonClicked: Back
            );
        }

        public void OpenBundleStore()
        {
            var bundleStoreUI = UIManager.Instance.Replace<BundleStoreUI>(ref currentUI);
            var products = IAPManager.Instance.GetAllBundleProducts();
            bundleStoreUI.Init(
                bundleProducts: products,
                onBackButtonClicked: OpenMainStore
            );
        }

        /// <summary>
        /// Avatars and outfits share one shelf. DOC 02's Shop tab lists both under the avatar
        /// store, which is why the mock-up hub labels this button Outfit while the code has always
        /// called it Avatar.
        /// </summary>
        public void OpenAvatarStore()
        {
            var itemStoreUI = UIManager.Instance.Replace<ItemStoreUI>(ref currentUI);

            var products = new List<IInGameProductMetadata>();
            products.AddRange(AvatarStoreProductsResourceManager.Instance.GetAllResources());
            products.AddRange(OutfitStoreProductsResourceManager.Instance.GetAllResources());

            itemStoreUI.Init(products.ToArray());
            itemStoreUI.OnClosed += OpenMainStore;
            itemStoreUI.OnPurchaseProduct += PurchaseProduct;
            ShowBalance(itemStoreUI);
        }

        public void OpenRoomStore()
        {
            var roomStoreUI = UIManager.Instance.Replace<RoomStoreUI>(ref currentUI);
            var allDecorationProducts = DecorationStoreProductsResourceManager.Instance.GetAllResources();
            roomStoreUI.OnClosed += OpenMainStore;
            roomStoreUI.Init(allDecorationProducts);
            roomStoreUI.OnPurchaseProduct += PurchaseProduct;
            ShowBalance(roomStoreUI);
        }

        public void OpenItemStore()
        {
            var itemStoreUI = UIManager.Instance.Replace<ItemStoreUI>(ref currentUI);
            var inGameProducts = ItemStoreProductsResourceManager.Instance.GetAllResources();
            itemStoreUI.Init(inGameProducts);
            itemStoreUI.OnClosed += OpenMainStore;
            itemStoreUI.OnPurchaseProduct += PurchaseProduct;
            ShowBalance(itemStoreUI);
        }

        /// <summary>
        /// Asks before spending anything. A purchase takes currency the player earned and cannot be
        /// undone, so a mistaken tap on a grid of look-alike tiles should not cost them a thing.
        /// </summary>
        public void PurchaseProduct(IInGameProductMetadata product)
        {
            if (product == null) return;

            var item = product.GetItemProduct();
            var itemName = item != null ? item.GetName() : product.GetID();
            var currencyType = product.GetPurchasedCurrency().GetCurrencyType();

            var confirm = UIManager.Instance.Create<ConfirmPopupUI>();
            confirm.Setup(
                "Trade Confirm",
                $"Do you want to buy {itemName} with {product.GetPrice()} {currencyType}?",
                onConfirmButtonClicked: () => { confirm.Close(); CompletePurchase(product); },
                onCancelButtonClicked: () => confirm.Close());
        }

        /// <summary>Runs only once the player has said yes.</summary>
        private async void CompletePurchase(IInGameProductMetadata product)
        {
            var currencyType = product.GetPurchasedCurrency().GetCurrencyType();
            var price = product.GetPrice();
            var productId = product.GetID();

            Debug.Log($"[Store] User purchase product ID: {productId}, currency: {currencyType}, price: {price}");

            // Try to spend the currency (checks balance internally).
            var spendSuccess = await PlayerDataManager.Instance.TrySpendCurrency(currencyType, price);
            if (!spendSuccess)
            {
                Debug.LogWarning($"[Store] Purchase failed: not enough {currencyType} for product '{productId}'.");
                // Saying nothing leaves the player tapping a button that appears to do nothing.
                var message = UIManager.Instance.Create<MessagePopupUI>();
                message.Setup(
                    "Not enough " + currencyType,
                    $"You need {price} {currencyType} to buy this.",
                    "OK",
                    onMessageButtonClicked: () => message.Close(),
                    tone: PopupTone.Alert);
                return;
            }

            // Grant the item to the player inventory.
            var item = product.GetItemProduct();
            if (item != null)
            {
                await PlayerDataManager.Instance.AddInventoryItem(
                    itemId: item.GetID(),
                    itemName: item.GetName(),
                    itemType: ((int)item.GetItemType()).ToString(),
                    quantity: 1
                );
                Debug.Log($"[Store] Granted item '{item.GetID()}' to player.");
            }

            // Tell whoever is showing a balance that it has changed. Without this the counter on
            // screen keeps the figure from before the purchase, and the player cannot tell whether
            // their money was taken.
            ShowBalance(currentUI);
            OnCurrencyChanged?.Invoke();
        }

        /// <summary>
        /// Puts the player's real balance on a shelf's currency strip.
        ///
        /// The strip has had working setters all along but nothing ever called them, so every shop
        /// screen showed the 9999999 left in the prefab at design time. A player deciding whether
        /// they can afford something was reading a number that had nothing to do with their money.
        /// </summary>
        private async void ShowBalance(BaseUI ui)
        {
            if (ui == null) return;

            var display = ui.GetComponentInChildren<GameCurrencyVisualization>(true);
            if (display == null) return;

            var inventory = await PlayerDataManager.Instance.GetInventory();

            // The shelf can be closed while the balance is still in flight.
            if (display == null) return;

            display.SetCoinAmount(inventory?.Coins ?? 0);
            display.SetGemAmount(inventory?.Gems ?? 0);
        }

        public void OpenTopUpStore()
        {
            var topUpStoreUI = UIManager.Instance.Replace<TopUpStoreUI>(ref currentUI);
            var products = IAPManager.Instance.GetAllTopUpProducts();
            topUpStoreUI.Init(
                products: products,
                onCloseButtonClicked: OpenMainStore
            );
        }
    }
}
