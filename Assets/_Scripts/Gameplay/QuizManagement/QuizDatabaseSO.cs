using System;
using System.Collections.Generic;
using System.Text;
using NaughtyAttributes;
using QuizGame.Destination;
using UnityEngine;
using UnityEngine.Localization;

namespace QuizGame.Gameplay.QuizManagement
{
    [CreateAssetMenu(fileName = "QuizDatabase", menuName = "QuizGame/QuizDatabase")]
    public class QuizDatabaseSO : ScriptableObject
    {
        [SerializeField]
        private string QuizLocalizationTableName = "Quiz";

        [SerializeField]
        private TextAsset quizDataFile;

        [SerializeField, ReadOnly]
        private string[] Headers;

        [SerializeField, ReadOnly]
        private List<QuizData> Quizzes;

        [Button("Load Quiz Data from QuizDataFile")]
        public void LoadQuiz()
        {
            if (quizDataFile == null)
            {
                Debug.LogError("Quiz data file not found");
                return;
            }

            var lines = quizDataFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var filteredLines = new List<string> { lines[0] }; // keep header
            for (int i = 1; i < lines.Length; i++)
            {
                var fields = lines[i].Split(',', StringSplitOptions.None);
                if (!string.IsNullOrWhiteSpace(fields[0]) && !string.IsNullOrWhiteSpace(fields[1]))
                {
                    filteredLines.Add(lines[i]);
                }
            }

            lines = filteredLines.ToArray();
            Debug.Log($"Total lines in CSV: {lines.Length}");

            Headers = lines[0].Split(',');
            Quizzes = new List<QuizData>(lines.Length - 1);

            for (int i = 1; i < lines.Length; i++)
            {
                var fields = SplitCsvLine(lines[i]);

                if (fields.Length < 11)
                {
                    Debug.LogError($"Data index {i} doesn't have enough column.");
                    continue;
                }

                if (!int.TryParse(fields[0], out int number))
                {
                    Debug.LogError($"Invalid difficulty level at line {i + 1}. Setting to 0.");
                    number = 0;
                }

                var question = fields[1];
                var questionLocalizedString = new LocalizedString(QuizLocalizationTableName, $"{number:D4}.question");
                var choices = new string[4] { fields[2], fields[3], fields[4], fields[5] };
                var choiceLocalizedString = new LocalizedString[]
                {
                    new LocalizedString(QuizLocalizationTableName, $"{number:D4}.choice1"),
                    new LocalizedString(QuizLocalizationTableName, $"{number:D4}.choice2"),
                    new LocalizedString(QuizLocalizationTableName, $"{number:D4}.choice3"),
                    new LocalizedString(QuizLocalizationTableName, $"{number:D4}.choice4")
                };

                if (!QuizHelper.TypeMapping.TryGetValue(fields[6], out var type))
                {
                    Debug.LogError($"Unknown quiz type '{fields[6]}' at line {i + 1}. Setting to '4 Choice'.");
                    Debug.Log($"{i} Data: " + string.Join(", ", fields));
                    type = QuizType.None;
                }

                if (!int.TryParse(fields[7], out int difficultyLevel))
                {
                    Debug.LogError($"Invalid difficulty level '{fields[7]}' at line {i + 1}. Setting to 0.");
                    Debug.Log($"{i} Data: " + string.Join(", ", fields));
                    difficultyLevel = 0;
                }

                if (!QuizHelper.CategoryMapping.TryGetValue(fields[8], out var category1))
                {
                    Debug.LogError($"Unknown category '{fields[8]}' at line {i + 1}. Setting to 'None'.");
                    Debug.Log($"{i} Data: " + string.Join(", ", fields));
                }

                if (!QuizHelper.CategoryMapping.TryGetValue(fields[9], out var category2))
                {
                    Debug.LogError($"Unknown category '{fields[9]}' at line {i + 1}. Setting to 'None'.");
                    Debug.Log($"{i} Data: " + string.Join(", ", fields));
                }

                if (!QuizHelper.DestinationMapping.TryGetValue(fields[10], out var location))
                {
                    if (!fields[10].Equals("#none"))
                    {
                        Debug.LogError($"Unknown location '{fields[10]}' at line {i + 1}. Setting to 'none'.");
                        Debug.Log($"{i} Data: " + string.Join(", ", fields));
                    }
                    location = DestinationType.None;
                }
                Quizzes.Add(new QuizData(number, question, questionLocalizedString, choices, choiceLocalizedString, type, (QuizDifficultyLevel)difficultyLevel, category1, category2, location));
            }

            Debug.Log($"Loaded {Quizzes.Count} quizzes from CSV.");
        }

        public List<QuizData> GetAllQuizzes()
        {
            return Quizzes;
        }

        public string[] GetHeaders()
        {
            return Headers;
        }

        public static string[] SplitCsvLine(string line)
        {
            var fields = new List<string>();
            var inQuotes = false;
            var currentField = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }
            fields.Add(currentField.ToString());
            return fields.ToArray();
        }
    }
}