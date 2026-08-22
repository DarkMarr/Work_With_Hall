using TMPro;
using UnityEngine;

namespace QuizGame.Gameplay.Quiz.DigitMode
{
    public class WaitingPanel : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI scoreText;

        public void SetScoreText(string score)
        {
            scoreText.text = score;
        }
    }
}
