using System;
using System.Collections.Generic;
using System.Linq;
using QuizGame.Item.UI;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Store.UI
{
    public class ItemStoreUI : ItemWithInfoSelectionUI
    {
        public event Action<IInGameProductMetadata> OnPurchaseProduct;

        [Header("Store")]
        [SerializeField]
        private ItemStoreBuyButton purchaseButton;

        [SerializeField]
        [Tooltip("Optional. Toggles named All, Coin, Gem, New or Season. Leave empty for no filter row.")]
        private ToggleGroup filterToggleGroup;

        private IInGameProductMetadata selectingProduct;
        private IInGameProductMetadata[] allProducts = new IInGameProductMetadata[0];

        protected override void Start()
        {
            base.Start();
            purchaseButton.OnPurchaseButtonClicked += () => OnPurchaseProduct?.Invoke(selectingProduct);
        }

        public void Init(IInGameProductMetadata[] inGameProducts)
        {
            allProducts = inGameProducts ?? new IInGameProductMetadata[0];

            OnSelectItem += (button, selectingItem) =>
            {
                if (selectingItem is IInGameProductMetadata inGameProduct)
                {
                    purchaseButton.Setup(inGameProduct);
                    selectingProduct = inGameProduct;
                }
            };

            HookFilterToggles();
            ShowFiltered();
        }

        /// <summary>
        /// Rebuilds the grid whenever a filter is picked. Each toggle is listened to individually
        /// because ToggleGroup has no changed event of its own.
        /// </summary>
        private void HookFilterToggles()
        {
            if (filterToggleGroup == null) return;
            foreach (var toggle in filterToggleGroup.GetComponentsInChildren<Toggle>(true))
            {
                var captured = toggle;
                captured.onValueChanged.AddListener(isOn =>
                {
                    // Turning one on turns another off, so only the new selection should redraw.
                    if (isOn) ShowFiltered();
                });
            }
        }

        private void ShowFiltered()
        {
            var filtered = RoomStoreUI.FilterItems(allProducts.Where(x => x != null).ToList(), CurrentFilter());

            // Keep the buy button on something real: the previous selection may have been filtered
            // away, and leaving it there would let the player buy an item they can no longer see.
            selectingProduct = filtered.FirstOrDefault();
            if (selectingProduct != null) purchaseButton.Setup(selectingProduct);

            base.Setup(0, filtered.ToArray());
        }

        /// <summary>
        /// Reads the chosen filter from the toggle names, the same way the room store does, so both
        /// stores can share one set of prefab conventions. No toggle group means show everything.
        /// </summary>
        private FilterType CurrentFilter()
        {
            if (filterToggleGroup == null) return FilterType.Any;

            var active = filterToggleGroup.ActiveToggles().FirstOrDefault();
            if (active == null) return FilterType.Any;

            switch (active.name)
            {
                case "New": return FilterType.New;
                case "Season": return FilterType.Season;
                case "Coin": return FilterType.Coin;
                case "Gem": return FilterType.Gem;
                case "All": return FilterType.Any;
                default:
                    Debug.LogWarning($"[ItemStoreUI] Filter toggle '{active.name}' is not a known filter; showing all.", this);
                    return FilterType.Any;
            }
        }
    }
}
