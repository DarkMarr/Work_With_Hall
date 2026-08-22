using QuizGame.UI;
using TMPro;
using UnityEngine;

namespace QuizGame.Gameplay.Quiz.SortMode
{
    public class DraggableAnswer : DraggableUI
    {
        public int AnswerID { get; private set; }
        public int CurrentOrder { get; private set; }

        [SerializeField]
        private TextMeshProUGUI answerText;

        public void Init(int answerID)
        {
            AnswerID = answerID;
        }

        public void SetOrder(int order)
        {
            CurrentOrder = order;
        }

        public void SetAnswerID(int id)
        {
            AnswerID = id;
        }

        public void SetText(string text)
        {
            answerText.text = text;
        }

        public string GetText() => answerText.text;
    }
}
