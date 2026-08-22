using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Matchmaking.UI
{
    [RequireComponent(typeof(Button))]
    public class PrepTimeSelectableItem : MonoBehaviour
    {
        public event Action OnItemButtonClicked;
    
        public bool IsSelected => selectedVisual.activeSelf;

        [SerializeField]
        private Button itemButton;

        [SerializeField]
        private GameObject selectedVisual;

        [SerializeField]
        private Image itemImage;

        [SerializeField]
        private TextMeshProUGUI itemQuantityText;

        private void Start()
        {
            itemButton.onClick.AddListener(() => OnItemButtonClicked?.Invoke());
        }

        public void SetIsSelected(bool isSelected)
        {
            selectedVisual.SetActive(isSelected);
        }

        public void SetItemImage(Sprite sprite)
        {
            itemImage.sprite = sprite;
        }

        public void SetItemQuantityText(string text)
        {
            itemQuantityText.text = text;
        }
    }
}
