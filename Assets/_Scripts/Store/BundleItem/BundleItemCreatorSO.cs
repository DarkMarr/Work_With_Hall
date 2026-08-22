using System;
using Newtonsoft.Json;
using QuizGame.Item;
using UnityEngine;

namespace QuizGame.Store
{
    [CreateAssetMenu(fileName = "NewBundleItem", menuName = "QuizGame/Store/BundleItem", order = 1)]
    public class BundleItemCreatorSO : ScriptableObject
    {
        [SerializeField]
        private ItemSOWithQuantityPair[] items;

        [SerializeField, TextArea(3, 10)]
        [Tooltip("JSON output of the bundle item data. Insert this to payouts data to define what item player will get in the bundle.")]
        private string jsonOutput;

        private void OnValidate()
        {
            jsonOutput = JsonConvert.SerializeObject(ConvertToDataArray(), Formatting.Indented);
        }

        private ItemWithQuantityPairData[] ConvertToDataArray()
        {
            var dataArray = new ItemWithQuantityPairData[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                dataArray[i] = new ItemWithQuantityPairData
                {
                    ItemID = item.Item.GetID(),
                    ItemType = (int)item.Item.GetItemType(),
                    Quantity = item.Quantity
                };
            }
            return dataArray;
        }
    }
}
