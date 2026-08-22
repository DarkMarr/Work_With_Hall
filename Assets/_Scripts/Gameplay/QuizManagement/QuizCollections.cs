using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuizGame.Destination;
using UnityEngine;

namespace QuizGame.Gameplay.QuizManagement
{
    public static class QuizCollections
    {
        public const string QuizDatabaseResourcePath = "QuizData/QuizDatabase";

        private static List<QuizData> quizzes;
        private static Dictionary<int, QuizData> quizzesByNumber = new Dictionary<int, QuizData>();

        public static async Task Initialize()
        {
            var quizDatabase = UnityEngine.Resources.LoadAsync<QuizDatabaseSO>(QuizDatabaseResourcePath);
            await quizDatabase;

            Debug.Log($"[{nameof(QuizCollections)}] Loaded QuizDatabase from path: {QuizDatabaseResourcePath}, which is done: {quizDatabase.isDone}, asset: {quizDatabase.asset != null}");

            if (quizDatabase.asset is QuizDatabaseSO quizDatabaseSO)
            {
                quizzes = quizDatabaseSO.GetAllQuizzes();
                quizzesByNumber.Clear();
                foreach (var quiz in quizzes)
                {
                    if (quiz.Number != -1)
                    {
                        if (!quizzesByNumber.ContainsKey(quiz.Number))
                        {
                            quizzesByNumber.Add(quiz.Number, quiz);
                        }
                        else
                        {
                            Debug.LogError($"Duplicate quiz number found: {quiz.Number}");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"Failed to load QuizDatabaseSO from path: {QuizDatabaseResourcePath}");
            }
        }

        public static List<QuizData> FilterByCategory(this List<QuizData> quizzes, QuizCategory category)
        {
            var filteredQuizzes = new List<QuizData>();
            foreach (var quiz in quizzes)
            {
                if (quiz.Category1 == category ||
                    quiz.Category2 == category)
                {
                    filteredQuizzes.Add(quiz);
                }
            }
            return filteredQuizzes;
        }

        public static List<QuizData> ExcludeByCategory(this List<QuizData> quizzes, QuizCategory category)
        {
            var filteredQuizzes = new List<QuizData>();
            foreach (var quiz in quizzes)
            {
                if (quiz.Category1 != category ||
                    quiz.Category2 != category)
                {
                    filteredQuizzes.Add(quiz);
                }
            }
            return filteredQuizzes;
        }

        public static List<QuizData> FilterByCategories(this List<QuizData> quizzes, params QuizCategory[] category)
        {
            var filteredQuizzes = new List<QuizData>();
            foreach (var quiz in quizzes)
            {
                if (category.Any(x => x == quiz.Category1 || x == quiz.Category2))
                {
                    filteredQuizzes.Add(quiz);
                }
            }
            return filteredQuizzes;
        }

        public static List<QuizData> ExcludeByCategories(this List<QuizData> quizzes, params QuizCategory[] category)
        {
            var excludedQuizzes = new List<QuizData>();
            foreach (var quiz in quizzes)
            {
                if (category.All(x => x != quiz.Category1 && x != quiz.Category2))
                {
                    excludedQuizzes.Add(quiz);
                }
            }
            return excludedQuizzes;
        }

        public static List<QuizData> FilterByDestination(this List<QuizData> quizzes, DestinationType location)
        {
            var filteredQuizzes = new List<QuizData>();
            foreach (var quiz in quizzes)
            {
                if (quiz.Location == location)
                {
                    filteredQuizzes.Add(quiz);
                }
            }
            return filteredQuizzes;
        }

        public static List<QuizData> ExcludeByDestination(this List<QuizData> quizzes, DestinationType location)
        {
            var filteredQuizzes = new List<QuizData>();
            foreach (var quiz in quizzes)
            {
                if (quiz.Location != location)
                {
                    filteredQuizzes.Add(quiz);
                }
            }
            return filteredQuizzes;
        }

        public static List<QuizData> FilterByQuizType(this List<QuizData> quizzes, params QuizType[] quizType)
        {
            var filteredQuizzes = new List<QuizData>();
            foreach (var quiz in quizzes)
            {
                if (quizType.Any(x => x == quiz.Type))
                {
                    filteredQuizzes.Add(quiz);
                }
            }
            return filteredQuizzes;
        }

        public static List<QuizData> ExcludeByQuizType(this List<QuizData> quizzes, params QuizType[] quizType)
        {
            var filteredQuizzes = new List<QuizData>();
            foreach (var quiz in quizzes)
            {
                if (quizType.Any(x => x != quiz.Type))
                {
                    filteredQuizzes.Add(quiz);
                }
            }
            return filteredQuizzes;
        }

        public static List<QuizData> FilterByDifficulty(this List<QuizData> quizzes, QuizDifficultyLevel difficultyLevel)
        {
            var filteredQuizzes = new List<QuizData>();
            foreach (var quiz in quizzes)
            {
                if (quiz.DifficultyLevel == difficultyLevel)
                {
                    filteredQuizzes.Add(quiz);
                }
            }
            return filteredQuizzes;
        }

        public static List<QuizData> ExcludeByDifficulty(this List<QuizData> quizzes, QuizDifficultyLevel difficultyLevel)
        {
            var filteredQuizzes = new List<QuizData>();
            foreach (var quiz in quizzes)
            {
                if (quiz.DifficultyLevel != difficultyLevel)
                {
                    filteredQuizzes.Add(quiz);
                }
            }
            return filteredQuizzes;
        }

        public static List<QuizData> GetAllQuizzes() => quizzes;

        public static int Count() => quizzes.Count;

        public static QuizData GetRandomQuiz(this List<QuizData> quizzes)
        {
            if (quizzes == null || quizzes.Count == 0)
            {
                Debug.LogError("No quizzes available.");
                return new QuizData();
            }
            var randomIndex = Random.Range(0, quizzes.Count);
            return quizzes[randomIndex];
        }

        public static QuizData GetQuizByNumber(int number)
        {
            if (!quizzesByNumber.TryGetValue(number, out var quiz))
            {
                Debug.LogError($"No quiz found with number: {number}");
                return new QuizData();
            }
            return quiz;
        }

        public static QuizData GetQuizByID(int id)
        {
            if (id > quizzes.Count - 1 || id < 0)
            {
                Debug.LogError($"No quiz found with ID: {id}");
                return new QuizData();
            }
            return quizzes[id];
        }
    }
}
