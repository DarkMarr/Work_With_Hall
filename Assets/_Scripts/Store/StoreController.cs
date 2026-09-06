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
        /// Handles purchasing an in-game product: checks currency balance, deducts cost, and grants the item.
        /// </summary>
        public async void PurchaseProduct(IInGameProductMetadata product)
        {
            if (product == null) return;

            var currencyType = product.GetPurchasedCurrency().GetCurrencyType();
            var price = product.GetPrice();
            var productId = product.GetID();

            Debug.Log($"[Store] User purchase product ID: {productId}, currency: {currencyType}, price: {price}");

            // Try to spend the currency (checks balance internally).
            var spendSuccess = await PlayerDataManager.Instance.TrySpendCurrency(currencyType, price);
            if (!spendSuccess)
            {
                Debug.LogWarning($"[Store] Purchase failed: not enough {currencyType} for product '{productId}'.");
                // TODO: Show "not enough currency" popup UI
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

            // TODO: Refresh any open store UI to reflect updated currency balance.
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
