using System;
using QuizGame.Item.UI;
using UnityEngine;

namespace QuizGame.Store.UI
{
    public class ItemStoreUI : ItemWithInfoSelectionUI
    {
        public event Action<IInGameProductMetadata> OnPurchaseProduct;

        [Header("Store")]
        [SerializeField]
        private ItemStoreBuyButton purchaseButton;

        private IInGameProductMetadata selectingProduct;

        protected override void Start()
        {
            base.Start();
            purchaseButton.OnPurchaseButtonClicked += () => OnPurchaseProduct?.Invoke(selectingProduct);
        }

        public void Init(IInGameProductMetadata[] inGameProducts)
        {
            if (inGameProducts.Length <= 0) return;

            var firstProduct = inGameProducts[0];
            purchaseButton.Setup(firstProduct);
            OnSelectItem += (button, selectingItem) =>
            {
                if (selectingItem is IInGameProductMetadata inGameProduct)
                {
                    purchaseButton.Setup(inGameProduct);
                    selectingProduct = inGameProduct;
                }
            };
            base.Setup(0, inGameProducts);
        }
    }
}
