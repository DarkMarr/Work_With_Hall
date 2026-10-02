using System;
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

        public void OpenAvatarStore()
        {
            var itemStoreUI = UIManager.Instance.Replace<ItemStoreUI>(ref currentUI);
            var avatarProducts = AvatarStoreProductsResourceManager.Instance.GetAllResources();
            itemStoreUI.Init(avatarProducts);
            itemStoreUI.OnClosed += OpenMainStore;
            itemStoreUI.OnPurchaseProduct += PurchaseProduct;
        }

        public void OpenRoomStore()
        {
            var roomStoreUI = UIManager.Instance.Replace<RoomStoreUI>(ref currentUI);
            var allDecorationProducts = DecorationStoreProductsResourceManager.Instance.GetAllResources();
            roomStoreUI.OnClosed += OpenMainStore;
            roomStoreUI.Init(allDecorationProducts);
            roomStoreUI.OnPurchaseProduct += PurchaseProduct;
        }

        public void OpenItemStore()
        {
            var itemStoreUI = UIManager.Instance.Replace<ItemStoreUI>(ref currentUI);
            var inGameProducts = ItemStoreProductsResourceManager.Instance.GetAllResources();
            itemStoreUI.Init(inGameProducts);
            itemStoreUI.OnClosed += OpenMainStore;
            itemStoreUI.OnPurchaseProduct += PurchaseProduct;
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
                    onMessageButtonClicked: () => message.Close());
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
            OnCurrencyChanged?.Invoke();
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
