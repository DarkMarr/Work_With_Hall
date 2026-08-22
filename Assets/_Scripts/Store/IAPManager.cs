using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QuizGame.Currency;
using QuizGame.Item;
using QuizGame.Material;
using QuizGame.MyRoom.Decoration;
using QuizGame.Utilities;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace QuizGame.Store
{
    public class IAPManager : MonoSingleton<IAPManager>, IDetailedStoreListener
    {
        public const string TopUpProductPrefix = "top_up";
        public const string BundleProductPrefix = "bundle";

        public IStoreController Store { get; private set; }
        public IExtensionProvider ExtensionProvider { get; private set; }

        public bool IsInitCompleted { get; private set; }

        [SerializeField]
        private bool UseFakeStore = false;

        private Dictionary<string, ProductCatalogPayout> payoutByProduct = new();

        public async Task InitAsync()
        {
            var options = new InitializationOptions()
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            .SetEnvironmentName("test");
#else
            .SetEnvironmentName("production");
#endif
            var serviceInitTask = UnityServices.InitializeAsync(options);
            var operation = UnityEngine.Resources.LoadAsync<TextAsset>("IAPProductCatalog");
            var tcs = new TaskCompletionSource<bool>();
            operation.completed += op =>
            {
                HandleIAPCatalogLoaded(op);
                tcs.SetResult(true);
            };
            await Task.WhenAll(serviceInitTask, tcs.Task);
        }

        private void HandleIAPCatalogLoaded(AsyncOperation Operation)
        {
            var request = Operation as ResourceRequest;

            Debug.Log($"[{GetType().Name}] Loaded Asset: {request.asset}");
            var catalog = JsonUtility.FromJson<ProductCatalog>((request.asset as TextAsset).text);
            Debug.Log($"[{GetType().Name}] Loaded catalog with {catalog.allProducts.Count} items");

            if (UseFakeStore)
            {
                StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.StandardUser; // Comment out this line if you are building the game for publishing.
                StandardPurchasingModule.Instance().useFakeStoreAlways = true; // Comment out this line if you are building the game for publishing.
            }

#if UNITY_ANDROID
            var builder = ConfigurationBuilder.Instance(
                StandardPurchasingModule.Instance(AppStore.GooglePlay)
            );
#elif UNITY_IOS
            var builder = ConfigurationBuilder.Instance(
                StandardPurchasingModule.Instance(AppStore.AppleAppStore)
            );
#else
            var builder = ConfigurationBuilder.Instance(
                StandardPurchasingModule.Instance(AppStore.NotSpecified)
            );
#endif

            foreach (var product in catalog.allProducts)
            {
                builder.AddProduct(product.id, product.type);
                payoutByProduct.Add(product.id, product.Payouts.FirstOrDefault());
            }

            Debug.Log($"[{GetType().Name}] Initializing Unity IAP with {catalog.allProducts.Count} products");
            UnityPurchasing.Initialize(this, builder);
        }

        public void Purchase(Product product, QuizGameIAPButton button)
        {
            if (!UseFakeStore)
            {
                Store.InitiatePurchase(product); //TODO: If we don't connect to Google Play Console or Apple App Store, it doesn't work.
            }

            //TODO: May replace with IDetailedStoreListener callbacks instead after connect with Google play console or Apple App Store.
            var iapButton = button.GetCodelessIAPButton();
            var productId = product.definition.id;

            iapButton.onOrderPending ??= new CodelessIAPButton.OnOrderPendingEvent();
            iapButton.onOrderPending.RemoveAllListeners();
            iapButton.onOrderPending.AddListener(order =>
            {
                var purchasedProduct = GetOrderedProduct(order, productId);
                if (purchasedProduct == null)
                {
                    Debug.LogWarning($"[{GetType().Name}] Pending order contains no cart item for {productId}");
                    button.OnPurchaseCompleted();
                    return;
                }

                Debug.Log($"[{GetType().Name}] Purchase completed for {purchasedProduct.definition.id}");
                var payout = GetProductPayout(purchasedProduct);

                switch (purchasedProduct.definition.type)
                {
                    case ProductType.Consumable:
                        Debug.Log($"[{GetType().Name}] Consumable product purchased: {purchasedProduct.definition.id}");
                        break;
                    case ProductType.NonConsumable:
                        Debug.Log($"[{GetType().Name}] Non-consumable product purchased: {purchasedProduct.definition.id}");
                        break;
                    case ProductType.Subscription:
                        Debug.Log($"[{GetType().Name}] Subscription product purchased: {purchasedProduct.definition.id}");
                        break;
                    default:
                        Debug.LogWarning($"[{GetType().Name}] Unknown product type: {purchasedProduct.definition.type}");
                        break;
                }

                switch (payout.type)
                {
                    case ProductCatalogPayout.ProductCatalogPayoutType.Item:
                        Debug.Log($"[{GetType().Name}] Item payout data: {payout.data}");
                        var itemData = JsonConvert.DeserializeObject<ItemWithQuantityPairData[]>(payout.data);
                        foreach (var item in itemData)
                        {
                            Debug.Log($"[{GetType().Name}] Item payout: {item.ItemID} ({(ItemType)item.ItemType}) -> {item.Quantity}");
                            //TODO: [Network] Add item to player's inventory (Database).
                        }
                        break;
                    case ProductCatalogPayout.ProductCatalogPayoutType.Currency:
                        Debug.Log($"[{GetType().Name}] Currency payout: {payout.subtype} -> {payout.quantity}");
                        break;
                    case ProductCatalogPayout.ProductCatalogPayoutType.Resource:
                        Debug.Log($"[{GetType().Name}] Resources payout: {payout.subtype} -> {payout.quantity}");
                        break;
                    default:
                        Debug.LogWarning($"[{GetType().Name}] Unknown payout type: {payout.type}");
                        break;
                }
                button.OnPurchaseCompleted();
            });
            iapButton.onPurchaseFailed ??= new CodelessIAPButton.OnPurchaseFailedEvent();
            iapButton.onPurchaseFailed.RemoveAllListeners();
            iapButton.onPurchaseFailed.AddListener(failedOrder =>
            {
                Debug.LogWarning($"[{GetType().Name}] Purchase failed for {productId} with reason: {failedOrder.FailureReason} ({failedOrder.Details})");
                button.OnPurchaseCompleted();
            });
        }

        private static Product GetOrderedProduct(Order order, string productId)
        {
            return order?.CartOrdered?.Items()
                .FirstOrDefault(item => item.Product.definition.id == productId)?.Product;
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Store = controller;
            ExtensionProvider = extensions;
            Debug.Log($"[{GetType().Name}] Successfully Initialized Unity IAP. Store Controller has {Store.products.all.Length} products");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"[{GetType().Name}] Error initializing IAP because of {error}.");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.Log($"[{GetType().Name}] Failed to purchase {product.definition.id} because {failureDescription}");
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log($"[{GetType().Name}] Failed to purchase {product.definition.id} because {failureReason}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            Debug.Log($"[{GetType().Name}] Successfully purchased {purchaseEvent.purchasedProduct.definition.id}");
            return PurchaseProcessingResult.Complete;
        }

        public ProductCatalogPayout GetProductPayout(Product product)
        {
            if (product == null || product.definition == null)
            {
                Debug.LogWarning($"[{GetType().Name}] Product or its definition is null.");
                return null;
            }
            return payoutByProduct.TryGetValue(product.definition.id, out var payout) ? payout : null;
        }

        public List<Product> GetProductsByPrefix(string prefix, bool isDescending = true)
        {
            if (Store == null || Store.products == null || Store.products.all == null)
            {
                Debug.LogWarning($"[{GetType().Name}] Store or products are not initialized.");
                return new List<Product>();
            }

            var products = Store.products.all
                .Where(product => product.definition.id.StartsWith(prefix))
                .OrderBy(product => isDescending ? -product.metadata.localizedPrice : product.metadata.localizedPrice)
                .ToList();

            return products;
        }

        public List<Product> GetAllTopUpProducts() => GetProductsByPrefix(TopUpProductPrefix);

        public List<Product> GetAllBundleProducts() => GetProductsByPrefix(BundleProductPrefix, false);
    }
}
