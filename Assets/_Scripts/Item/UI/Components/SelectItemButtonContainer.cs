using System.Collections.Generic;
using QuizGame.Interfaces;
using UnityEngine;

namespace QuizGame.Item.UI
{
    public class SelectItemButtonContainer : MonoBehaviour
    {
        public delegate void OnSelectButtonClicked(SelectItemButton targetButton, int buttonIndex);

        public SelectItemButton CurrentSelectingButton { get; private set; }

        private List<SelectItemButton> spawnedButtons;

        [SerializeField]
        private SelectItemButton selectItemButtonPrefab;

        [SerializeField]
        private Transform buttonContainer;

        /// <summary>
        /// Create "selectItemButton" for each sprites and save it to buttonContainer.
        /// </summary>
        /// <param name="itemSprites"></param>
        /// <param name="onSelectItemButtonClicked"></param>
        public void Init(IHasSprite[] itemSprites, OnSelectButtonClicked onSelectItemButtonClicked)
        {
            var itemButtonAmount = itemSprites.Length;
            spawnedButtons = new List<SelectItemButton>(itemButtonAmount);
            for (int i = 0; i < itemButtonAmount; i++)
            {
                var newButton = Instantiate(selectItemButtonPrefab, buttonContainer);
                spawnedButtons.Add(newButton);

                var buttonIndex = i;
                newButton.Init(
                    itemSprite: itemSprites[buttonIndex] == null ? null : itemSprites[buttonIndex].GetSprite(),
                    onItemButtonClicked: () =>
                    {
                        onSelectItemButtonClicked?.Invoke(newButton, buttonIndex);
                        CurrentSelectingButton = newButton;
                    });
            }
        }

        public SelectItemButton GetButtonAtIndex(int index)
        {
            if (index < 0 || index > spawnedButtons.Count - 1)
            {
                Debug.LogError("Button out of range");
                return null;
            }
            return spawnedButtons[index];
        }

        public void SetButtonSprite(int buttonIndex, IHasSprite sprite)
        {
            spawnedButtons[buttonIndex].SetSprite(sprite != null ? sprite.GetSprite() : null);
        }
    }
}
