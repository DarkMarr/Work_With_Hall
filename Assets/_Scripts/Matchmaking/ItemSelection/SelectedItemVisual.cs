using QuizGame.Item.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Matchmaking.UI
{
    public class SelectedItemVisual : MonoBehaviour
    {
        public IItem SelectedItem { get; private set; }
        public bool HasItem => itemImage.enabled;

        [SerializeField]
        private Image itemImage;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private Sprite nonItemBGSprite;

        [SerializeField]
        private Sprite hasItemBGSprite;

        private void Awake()
        {
            ClearItem();
        }

        public void SetItem(IItem item)
        {
            if (item != null)
            {
                itemImage.sprite = item.GetSprite();
                itemImage.enabled = true;
                backgroundImage.sprite = hasItemBGSprite;
                SelectedItem = item;
                return;
            }
            ClearItem();
        }

        public void ClearItem()
        {
            SelectedItem = null;
            itemImage.enabled = false;
            backgroundImage.sprite = nonItemBGSprite;
        }
    }
}
