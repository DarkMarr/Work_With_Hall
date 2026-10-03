using System;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Item.UI
{
    [RequireComponent(typeof(Button))]
    public class SelectItemButton : MonoBehaviour
    {
        private event Action onItemButtonClicked;

        public struct InitData
        {
            public Sprite ItemSprite;
            public Action OnItemButtonClicked;

            public InitData(Sprite itemSprite, Action onItemButtonClicked)
            {
                ItemSprite = itemSprite;
                OnItemButtonClicked = onItemButtonClicked;
            }
        }

        [SerializeField]
        private Button button;

        [SerializeField]
        private Image itemImage;

        private void OnValidate()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
        }

        private void Start()
        {
            button.onClick.AddListener(() => onItemButtonClicked?.Invoke());
        }

        public virtual void Init(InitData info)
        {
            Init(info.ItemSprite, info.OnItemButtonClicked);
        }

        public virtual void Init(Sprite itemSprite, Action onItemButtonClicked)
        {
            SetSprite(itemSprite);
            this.onItemButtonClicked = onItemButtonClicked;
        }

        public void SetSprite(Sprite itemSprite)
        {
            itemImage.sprite = itemSprite;
            // An Image with no sprite still draws, as a white box over the empty cell. It only
            // shows on a part-filled page, which is why it went unnoticed while every page was full.
            itemImage.enabled = itemSprite != null;
        }

        public bool HasItem() => itemImage.sprite != null;
    }
}
