using System;
using QuizGame.Material;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.MyRoom.Trade
{
    public class CompletedTradeSlot : MonoBehaviour
    {
        public event Action OnCollectButtonClicked;

        [SerializeField]
        private TextMeshProUGUI playerNameText;

        [SerializeField]
        private TextMeshProUGUI tradeNumberText;

        [SerializeField]
        private Image requestedItemImage;

        [SerializeField]
        private Button collectButton;

        void Start()
        {
            collectButton.onClick.AddListener(() => OnCollectButtonClicked?.Invoke());
        }

        public void Init(int slotNumber, string fulfilledPlayerName, IMaterial requestedMaterial)
        {
            tradeNumberText.text = slotNumber.ToString();
            requestedItemImage.sprite = requestedMaterial.GetSprite();
            playerNameText.text = $"<b>{fulfilledPlayerName}";
        }

        public void Close()
        {
            Destroy(gameObject);
        }
    }
}
