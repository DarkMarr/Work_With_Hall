using System;
using System.Collections.Generic;
using System.Linq;
using QuizGame.Item.UI;
using QuizGame.MyRoom.Decoration;
using QuizGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Store.UI
{
    public class RoomStoreUI : BaseItemSelectionUI<IInGameProductMetadata>
    {
        public event Action<IInGameProductMetadata> OnPurchaseProduct;
        public event Action OnOkayButtonClicked;
        public event Action OnDeselectButtonClicked;

        [SerializeField]
        private UIHighlighter highlighter;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private ToggleGroup decorationsToggleGroup;

        [SerializeField]
        private ToggleGroup filterToggleGroup;

        [Header("Staging UI")]
        [SerializeField]
        private GameObject itemInformationVisualization;

        [SerializeField]
        private GameObject NoItemVisualization;

        [Header("Current Item")]
        [SerializeField]
        private Image itemImage;

        [SerializeField]
        private Image itemPreviewImage;

        [SerializeField]
        private TextMeshProUGUI itemNameText;

        [Header("Store")]
        [SerializeField]
        private ItemStoreBuyButton purchaseButton;

        private IInGameProductMetadata selectingProduct;
        private List<IInGameProductMetadata> roomItems = new List<IInGameProductMetadata>();
        private List<IInGameProductMetadata> shelfItems = new List<IInGameProductMetadata>();
        private List<IInGameProductMetadata> floorItems = new List<IInGameProductMetadata>();
        private List<IInGameProductMetadata> wallItems = new List<IInGameProductMetadata>();

        protected virtual void Start()
        {
            closeButton?.onClick.AddListener(Close);
            purchaseButton.OnPurchaseButtonClicked += () => OnPurchaseProduct?.Invoke(selectingProduct);
            pageCategorySelectionButtons.OnIDChange += currentPage =>
            {
                highlighter.SetActive(SelectingItemInPageIndex == currentPage && SelectingItemIndex > -1 && selectingProduct != null);
            };
        }

        public void Init(IInGameProductMetadata[] allItems)
        {
            purchaseButton.Setup(null);
            OnSelectItem += (button, selectingItem) =>
            {
                if (selectingItem is IInGameProductMetadata inGameProduct)
                {
                    purchaseButton.Setup(inGameProduct);
                    selectingProduct = inGameProduct;
                }
            };

            foreach (var item in allItems)
            {
                if (item == null) continue;

                if (item.GetItemProduct() is not IDecorationItem decorationItem)
                {
                    Debug.LogWarning($"Item: {item.GetName()} is not a decoration, it is {item.GetItemType()}, skipping.");
                    continue;
                }

                switch (decorationItem.GetDecorationType())
                {
                    case DecorationType.Room:
                        roomItems.Add(item);
                        break;
                    case DecorationType.ShelfTrophy:
                        shelfItems.Add(item);
                        break;
                    case DecorationType.FloorTrophy:
                        floorItems.Add(item);
                        break;
                    case DecorationType.WallTrophy:
                        wallItems.Add(item);
                        break;
                    default:
                        Debug.LogWarning($"Unknown category for item: {item.GetName()}");
                        break;
                }
            }

            foreach (var toggle in decorationsToggleGroup.GetComponentsInChildren<Toggle>())
            {
                toggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                    {
                        OnDecorationToggleChanged();
                    }
                });
            }

            foreach (var toggle in filterToggleGroup.GetComponentsInChildren<Toggle>())
            {
                toggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                    {
                        OnDecorationToggleChanged();
                    }
                });
            }
            Refresh();
        }

        public void Refresh()
        {
            OnDecorationToggleChanged();
        }

        private void OnDecorationToggleChanged()
        {
            var currentActiveDecorationToggle = decorationsToggleGroup.ActiveToggles().FirstOrDefault();
            switch (currentActiveDecorationToggle.name)
            {
                case "Room":
                    ShowProducts(roomItems);
                    break;
                case "Shelf":
                    ShowProducts(shelfItems);
                    break;
                case "Floor":
                    ShowProducts(floorItems);
                    break;
                case "Wall":
                    ShowProducts(wallItems);
                    break;
                default:
                    Debug.LogWarning($"Decoration unknown toggle: {currentActiveDecorationToggle.name}");
                    break;
            }
        }

        private void ShowProducts(List<IInGameProductMetadata> showingItems)
        {
            var currentActiveFilterToggle = filterToggleGroup.ActiveToggles().FirstOrDefault();
            var filteredItems = showingItems.Where(item => item != null).ToList();
            var requiredFilter = FilterType.Any;
            switch (currentActiveFilterToggle.name)
            {
                case "New":
                    requiredFilter = FilterType.New;
                    break;
                case "Season":
                    requiredFilter = FilterType.Season;
                    break;
                case "Coin":
                    requiredFilter = FilterType.Coin;
                    break;
                case "Gem":
                    requiredFilter = FilterType.Gem;
                    break;
                case "All":
                    requiredFilter = FilterType.Any;
                    break;
                default:
                    requiredFilter = FilterType.Any;
                    Debug.LogWarning($"Filter unknown toggle: {currentActiveFilterToggle.name}");
                    break;
            }
            filteredItems = FilterItems(showingItems, requiredFilter);
            base.Setup(0, filteredItems.ToArray()); //First decor + first filter
        }

        public static List<IInGameProductMetadata> FilterItems(List<IInGameProductMetadata> items, FilterType filter)
        {
            if (filter == FilterType.Any || filter == FilterType.None)
            {
                return items;
            }
            return items.Where(item => (item.GetFilterType() & filter) != FilterType.None).ToList();
        }

        public override void SetNullCurrentItemVisualization()
        {
            itemImage.sprite = null;
            itemPreviewImage.sprite = null;
            itemNameText.text = "";

            highlighter.Hide();
            selectingProduct = null;
            purchaseButton.Setup(null);
            SetItemInfoUIActive(false);
        }

        public override void SetSelectingItemVisualization(SelectItemButton button, IInGameProductMetadata itemInfo)
        {
            if (itemInfo == null)
            {
                SetNullCurrentItemVisualization();
                return;
            }
            itemImage.sprite = itemInfo.GetSprite();
            itemNameText.text = itemInfo.GetName();
            itemPreviewImage.sprite = itemInfo.GetSprite();

            var clickedButtonRect = button.GetComponent<RectTransform>();
            selectingProduct = itemInfo;
            highlighter.Show();
            highlighter.SetTarget(clickedButtonRect);
            purchaseButton.Setup(itemInfo);
            SetItemInfoUIActive(true);
        }

        private void SetItemInfoUIActive(bool isActive)
        {
            itemInformationVisualization.SetActive(isActive);
            NoItemVisualization.SetActive(!isActive);
        }
    }
}
