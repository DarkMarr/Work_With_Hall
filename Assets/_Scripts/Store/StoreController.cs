using System;
using QuizGame.Store.UI;
using QuizGame.UI;
using UnityEngine;

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
            // var avatarStoreUI = UIManager.Instance.Replace<AvatarStoreUI>(ref currentUI);
            // avatarStoreUI.Init(
            //     onBackButtonClicked: OpenMainStore
            // );
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

        public void PurchaseProduct(IInGameProductMetadata product)
        {
            Debug.Log("Purchase");
            if (product == null) return;
            //TODO: [Network] Subtract player currency here and give item
            var currencyType =  product.GetPurchasedCurrency().GetCurrencyType();
            var price =  product.GetPrice();
            var id =  product.GetID();
            Debug.Log($"[Store] User purchase product ID: {id}, currency: {currencyType}, price: {price}");
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
