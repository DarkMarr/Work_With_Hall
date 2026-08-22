using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuizGame.Gameplay.Quiz.DigitMode;
using QuizGame.Gameplay.Quiz.FourChoicesMode;
using QuizGame.Gameplay.Quiz.SortMode;
using QuizGame.Gameplay.Quiz.TrueFalseMode;
using QuizGame.Gameplay.QuizManagement;
using QuizGame.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace QuizGame.Gameplay.Quiz
{
    public class QuizController : MonoBehaviour
    {
        private const int NETWORK_DELAY_MS = 2000;
        private const string TRUE_ANSWER = "TRUE";
        private const string FALSE_ANSWER = "FALSE";

        public event Action onSubmitAnswerButtonClicked;
        public bool QuizCompleted { get; private set; } = false;

        private Transform quizUIContainer;
        private BaseQuizModeUI currentQuizUI;

        public void SetQuizContainer(Transform quizContainer)
        {
            quizUIContainer = quizContainer;
        }

        public T ReplaceQuiz<T>() where T : BaseQuizModeUI
        {
            BaseUI onGoingQuizBaseUI = null;
            UIManager.Instance.Replace<T>(ref onGoingQuizBaseUI, quizUIContainer);
            currentQuizUI = onGoingQuizBaseUI as BaseQuizModeUI;
            return (T)currentQuizUI;
        }

        public void DisableCurrentQuizInteraction()
        {
            if (currentQuizUI != null)
            {
                currentQuizUI.SetInteractable(false);
            }
            QuizCompleted = true;
        }

        public void CloseCurrentQuiz()
        {
            if (currentQuizUI != null)
            {
                currentQuizUI.Close();
            }
            QuizCompleted = true;
        }

        public IEnumerator StartQuizCoroutine(QuizData quizData, Action<bool> onQuizAnswered = null)
        {
            yield return quizData.Type switch
            {
                QuizType.FourChoices => StartFourChoicesModeQuizCoroutine(quizData, onQuizAnswered),
                QuizType.Sorting => StartSortModeQuizCoroutine(quizData, onQuizAnswered),
                QuizType.TrueFalse => StartTrueFalseModeQuizCoroutine(quizData, onQuizAnswered),
                QuizType.NumberGuessing => StartEnterDigitModeQuizCoroutine(quizData, onQuizAnswered),
                _ => HandleUnsupportedQuizType(quizData.Type)
            };
        }

        private IEnumerator HandleUnsupportedQuizType(QuizType quizType)
        {
            Debug.LogError($"[QuizController] StartQuestionSequence - Unsupported quiz type: {quizType}");
            yield break;
        }

        private string[] ShuffleArray(string[] array)
        {
            return array.OrderBy(choice => Random.Range(0f, 1f)).ToArray();
        }

        #region EnterDigit
        public IEnumerator StartEnterDigitModeQuizCoroutine(QuizData quizData, Action<bool> onQuizAnswered = null)
        {
            QuizCompleted = false;
            var quizUI = ReplaceQuiz<QuizEnterDigitModeUI>();
            var correctAnswer = quizData.GetChoicesLocalize()[0];

            Debug.Log($"[GameplayController] ShowQuiz - Correct Answer: {correctAnswer}");

            var information = new QuizEnterDigitModeUI.Info()
            {
                OnSubmitButtonClicked = async (resultDigit) => await HandleDigitSubmission(quizUI, resultDigit, correctAnswer, onQuizAnswered)
            };

            quizUI.Init(information);
            yield return new WaitUntil(() => QuizCompleted);
        }

        private async Task HandleDigitSubmission(QuizEnterDigitModeUI quizUI, string resultDigit, string correctAnswer, Action<bool> onQuizAnswered)
        {
            onSubmitAnswerButtonClicked?.Invoke();
            Debug.Log($"[QuizEnterDigitMode] User enter digit: {resultDigit}");

            quizUI.ShowWaitingPanel(resultDigit);
            await Task.Delay(NETWORK_DELAY_MS); //TODO: [Network] Replace with wait for all player input / Time up

            var isCorrect = resultDigit == correctAnswer;
            onQuizAnswered?.Invoke(isCorrect);

            quizUI.ShowResultPanel();
            await Task.Delay(NETWORK_DELAY_MS);

            DisableCurrentQuizInteraction();
            QuizCompleted = true;
        }
        #endregion

        #region ForChoices
        public IEnumerator StartFourChoicesModeQuizCoroutine(QuizData quizData, Action<bool> onQuizAnswered = null)
        {
            QuizCompleted = false;
            var quiz = ReplaceQuiz<FourChoicesModeUI>();
            var correctAnswer = quizData.GetChoicesLocalize()[0];
            var shuffledChoices = ShuffleArray(quizData.GetChoicesLocalize());

            Debug.Log($"[GameplayController] ShowFourChoicesModeQuiz - Correct Answer: {correctAnswer} has choice index: {string.Join(",", shuffledChoices)}");

            var correctIndex = Array.IndexOf(shuffledChoices, correctAnswer);
            var information = new FourChoicesModeUI.Info()
            {
                ButtonMessage = shuffledChoices,
                OnSubmitAnswer = (answerID) => HandleFourChoicesAnswer(answerID, correctIndex, onQuizAnswered)
            };

            quiz.Init(information);
            yield return new WaitUntil(() => QuizCompleted);
        }

        private void HandleFourChoicesAnswer(int answerID, int correctAnswerIndex, Action<bool> onQuizAnswered)
        {
            onSubmitAnswerButtonClicked?.Invoke();

            var isCorrect = answerID == correctAnswerIndex;
            onQuizAnswered?.Invoke(isCorrect);

            DisableCurrentQuizInteraction();
            QuizCompleted = true;
        }
        #endregion

        #region SortQuiz
        public IEnumerator StartSortModeQuizCoroutine(QuizData quizData, Action<bool> onQuizAnswered = null)
        {
            QuizCompleted = false;
            var quiz = ReplaceQuiz<SortModeUI>();
            var correctOrder = quizData.GetChoicesLocalize();
            var shuffledOrder = ShuffleArray(quizData.GetChoicesLocalize());

            Debug.Log($"[GameplayController] ShowSortModeQuiz - Correct Order: {string.Join(",", correctOrder)} has shuffled order: {string.Join(",", shuffledOrder)}");

            var information = new SortModeUI.Info()
            {
                AnswerTexts = shuffledOrder,
                OnSubmitAnswerOrder = answerOrder => HandleSortModeAnswer(answerOrder, correctOrder, onQuizAnswered)
            };

            quiz.Init(information);
            yield return new WaitUntil(() => QuizCompleted);
        }

        private void HandleSortModeAnswer(Dictionary<int, DraggableAnswer> answerOrder, string[] correctOrder, Action<bool> onQuizAnswered)
        {
            onSubmitAnswerButtonClicked?.Invoke();

            var isCorrect = IsCorrectOrder(answerOrder, correctOrder);
            onQuizAnswered?.Invoke(isCorrect);

            DisableCurrentQuizInteraction();
            QuizCompleted = true;
        }

        private bool IsCorrectOrder(Dictionary<int, DraggableAnswer> answerOrder, string[] correctOrder)
        {
            for (int i = 0; i < answerOrder.Count; i++)
            {
                if (answerOrder[i].GetText() != correctOrder[i])
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region TrueFalseQuiz
        public IEnumerator StartTrueFalseModeQuizCoroutine(QuizData quizData, Action<bool> onQuizAnswered = null)
        {
            QuizCompleted = false;
            var quiz = ReplaceQuiz<TrueFalseModeController>();
            var correctAnswer = quizData.GetChoicesLocalize()[0];

            Debug.Log($"[GameplayController] ShowQuiz - Correct Answer: {correctAnswer}");

            var information = new TrueFalseModeController.Info()
            {
                OnAnswerButtonClicked = (answerTrue) => HandleTrueFalseAnswer(answerTrue, correctAnswer, onQuizAnswered)
            };

            quiz.Init(information);
            yield return new WaitUntil(() => QuizCompleted);
        }

        private void HandleTrueFalseAnswer(bool answerTrue, string correctAnswer, Action<bool> onQuizAnswered)
        {
            onSubmitAnswerButtonClicked?.Invoke();

            bool isCorrect = (correctAnswer == TRUE_ANSWER && answerTrue) ||
                           (correctAnswer == FALSE_ANSWER && !answerTrue);
            onQuizAnswered?.Invoke(isCorrect);

            DisableCurrentQuizInteraction();
            QuizCompleted = true;
        }
        #endregion
    }
}