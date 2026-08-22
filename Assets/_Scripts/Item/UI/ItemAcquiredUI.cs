using QuizGame.Item.Interfaces;
using QuizGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Item.UI
{
    public class ItemAcquiredUI : BaseUI
    {
        [SerializeField]
        private ImageWithTextVisualization itemVisualizationPrefab;

        [SerializeField]
        private Transform container;

        [SerializeField]
        private Button okButton;

        private void Start()
        {
            okButton.onClick.AddListener(Close);
        }

        public void Init(IQuantifiableItem[] items)
        {
            foreach (var item in items)
            {
                var newVisualization = Instantiate(itemVisualizationPrefab, container);
                newVisualization.Setup(item.GetSprite(), $"x{item.GetQuantity()}");
            }
        }
    }
}
