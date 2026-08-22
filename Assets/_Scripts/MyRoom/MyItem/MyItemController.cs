using System;
using QuizGame.Item.Interfaces;
using QuizGame.UI;
using UnityEngine;

namespace QuizGame.MyRoom.MyItem
{
    [Serializable]
    public class MyItemController
    {
        private MyItemModel model;

        private MyItemUI myItemUI;

        public void Init(MyItemModel model)
        {
            this.model = model;
            ShowMyItemUI();
        }

        private void ShowMyItemUI()
        {
            myItemUI = UIManager.Instance.Create<MyItemUI>();
            myItemUI.Init(
                trophyItems: model.TrophyItems,
                equipmentItems: model.EquipmentItems,
                consumableItems: model.ConsumableItems
            );
            myItemUI.OnRemoveItem += HandleItemRemoved;
            myItemUI.Show();
        }

        private void HandleItemRemoved(IItem item)
        {
            if (item is not IRecyclable recyclableItem)
                return;

            Debug.Log($"You recycled: {item.GetName()}");
            foreach (var gotItem in recyclableItem.GetRecycledItems())
            {
                Debug.Log($"Recycle: {item.GetName()}, You got: {gotItem.GetName()} x{gotItem.GetQuantity()}");
            }
            model.RemoveItem(item);
            myItemUI?.Refresh();
        }
    }
}
