using TMPro;
using UnityEngine;

namespace QuizGame.Store.UI
{
    public class GameCurrencyVisualization : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI gemAmountText;

        [SerializeField]
        private TextMeshProUGUI coinAmountText;

        public void SetGemAmount(int amount)
        {
            gemAmountText.text = amount.ToString();
        }

        public void SetCoinAmount(int amount)
        {
            coinAmountText.text = amount.ToString();
        }
    }
}
