using System.Collections.Generic;
using Newtonsoft.Json;
using QuizGame.Currency;
using QuizGame.Item;
using QuizGame.Material;
using QuizGame.MyRoom.Decoration;
using UnityEngine;
using UnityEngine.Purchasing;

namespace QuizGame.Store
{
    public static class ItemHelper
    {
        public static IEnumerable<ItemWithQuantityPair> GetItemsWithQuantityFromPayoutData(this Product product)
        {
            var payout = IAPManager.Instance.GetProductPayout(product);
            return GetItemsWithQuantityFromDataJson(payout.data);
        }

        public static IEnumerable<ItemWithQuantityPair> GetItemsWithQuantityFromData(ItemWithQuantityPairData[] itemData)
        {
            foreach (var data in itemData)
            {
                if (TryGetItemFromData(data, out var item))
                {
                    yield return item;
                }
                else
                {
                    Debug.LogWarning($"Failed to get item from data: {JsonConvert.SerializeObject(data)}");
                }
            }
        }

        public static IEnumerable<ItemWithQuantityPair> GetItemsWithQuantityFromDataJson(string json)
        {
            var itemData = JsonConvert.DeserializeObject<ItemWithQuantityPairData[]>(json);
            foreach (var data in itemData)
            {
                if (TryGetItemFromData(data, out var item))
                {
                    yield return item;
                }
                else
                {
                    Debug.LogWarning($"Failed to get item from data: {JsonConvert.SerializeObject(data)}");
                }
            }
        }

        public static bool TryGetItemFromDataJson(string json, out ItemWithQuantityPair item)
        {
            var itemData = JsonConvert.DeserializeObject<ItemWithQuantityPairData>(json);
            return TryGetItemFromData(itemData, out item);
        }

        public static bool TryGetItemFromData(ItemWithQuantityPairData itemData, out ItemWithQuantityPair item)
        {
            item = null;

            switch ((ItemType)itemData.ItemType)
            {
                case ItemType.Consumable:
                    item = new ItemWithQuantityPair(
                        CarryOnItemResourceManager.Instance.GetResource(itemData.ItemID),
                        itemData.Quantity
                    );
                    break;

                case ItemType.Currency:
                    item = new ItemWithQuantityPair(
                        CurrencyResourceManager.Instance.GetResource(itemData.ItemID),
                        itemData.Quantity
                    );
                    break;

                case ItemType.Decoration:
                    item = new ItemWithQuantityPair(
                        DecorationItemResourceManager.Instance.GetResource(itemData.ItemID),
                        itemData.Quantity
                    );
                    break;

                case ItemType.Material:
                    item = new ItemWithQuantityPair(
                        MaterialResourceManager.Instance.GetResource(itemData.ItemID),
                        itemData.Quantity
                    );
                    break;

                default:
                    Debug.LogWarning($"[Product] Unknown item type: {itemData.ItemType}");
                    return false;
            }
            return item?.GetName() != string.Empty;
        }
    }
}
