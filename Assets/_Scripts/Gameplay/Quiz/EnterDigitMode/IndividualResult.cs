using TMPro;
using UnityEngine;

namespace QuizGame.Gameplay.Quiz.DigitMode
{
    public class IndividualResult : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI rankText;

        [SerializeField]
        private TextMeshProUGUI playerNameText;

        [SerializeField]
        private TextMeshProUGUI answerTimeText;

        [SerializeField]
        private TextMeshProUGUI scoreText;

        public void Init(int rank, string playerName, string answerTime, int score)
        {
            rankText.text = rank.ToString();
            playerNameText.text = playerName;
            answerTimeText.text = answerTime;
            scoreText.text = score.ToString();
        }
    }
}
