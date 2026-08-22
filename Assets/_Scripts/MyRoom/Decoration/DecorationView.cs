using System;
using UnityEngine;

namespace QuizGame.MyRoom.Decoration
{
    [Serializable]
    public class DecorationView
    {
        [SerializeField]
        private DecorationSlot[] decorationSlots;

        public void InitSlots(Action<DecorationSlot> onChangeClicked)
        {
            foreach (var slot in decorationSlots)
            {
                slot.Init(() => onChangeClicked?.Invoke(slot));
                slot.SetChangeSpriteButtonActive(false);
            }
        }

        public void SetSlotSprite(string slotID, Sprite sprite)
        {
            var slot = GetSlotByID(slotID);
            if (slot != null)
            {
                slot.SetSprite(sprite);
            }
        }

        public void SetChangeDecorateButtonsActive(bool isActive)
        {
            foreach (var slot in decorationSlots)
            {
                slot.SetChangeSpriteButtonActive(isActive);
            }
        }

        public DecorationSlot[] GetSlots() => decorationSlots;

        public DecorationSlot GetSlotByID(string slotID)
        {
            foreach (var slot in decorationSlots)
            {
                if (slot.GetID() == slotID)
                    return slot;
            }
            return null;
        }
        
        public void AutoFindSlots()
        {
            decorationSlots = GameObject.FindObjectsByType<DecorationSlot>(FindObjectsSortMode.None);
        }
    }

}
