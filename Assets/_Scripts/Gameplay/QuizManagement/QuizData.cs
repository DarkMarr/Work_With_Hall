using System;
using QuizGame.Destination;
using UnityEngine;
using UnityEngine.Localization;

namespace QuizGame.Gameplay.QuizManagement
{
    [Serializable]
    public struct QuizData
    {
        public const int CorrectChoiceIndex = 0; // always the first choice is correct

        public int Number;

        [SerializeField]
        private string question;

        [SerializeField]
        private LocalizedString questionLocalizedString;

        [SerializeField]
        private string[] choices;

        [SerializeField]
        private LocalizedString[] choiceLocalizedStrings;

        public QuizType Type;
        public QuizDifficultyLevel DifficultyLevel;
        public QuizCategory Category1;
        public QuizCategory Category2;
        public DestinationType Location;

        public QuizData(int number = -1)
        {
            Number = number;
            question = string.Empty;
            questionLocalizedString = null;
            choices = Array.Empty<string>();
            choiceLocalizedStrings = Array.Empty<LocalizedString>();
            Type = QuizType.None;
            DifficultyLevel = QuizDifficultyLevel.Easy;
            Category1 = QuizCategory.None;
            Category2 = QuizCategory.None;
            Location = DestinationType.None;
        }

        public QuizData(int number, string question, LocalizedString questionLocalizedString, string[] choices, LocalizedString[] choiceLocalizedString, QuizType type, QuizDifficultyLevel difficultyLevel, QuizCategory category1, QuizCategory category2, DestinationType location)
        {
            Number = number;
            this.question = question;
            this.questionLocalizedString = questionLocalizedString;
            this.choiceLocalizedStrings = choiceLocalizedString;
            this.choices = choices;
            Type = type;
            DifficultyLevel = difficultyLevel;
            Category1 = category1;
            Category2 = category2;
            Location = location;

            this.choices = ValidateChoices(ref type, choices);
            this.choiceLocalizedStrings = ValidateChoicesLocalizedString(ref type, choices, choiceLocalizedString);
        }

        /// <summary>
        /// Get choices by their localize, but if their localize aren't enable, we will get the default value from CSV instead.
        /// </summary>
        /// <returns></returns>
        public string[] GetChoicesLocalize()
        {
            switch (Type)
            {
                case QuizType.TrueFalse:
                case QuizType.NumberGuessing:
                    return choices;
            }

            if (choiceLocalizedStrings == null || choiceLocalizedStrings.Length <= 0)
                return choices;

            var choicesLocalized = new string[choiceLocalizedStrings.Length];
            for (int i = 0; i < choiceLocalizedStrings.Length; i++)
            {
                var choiceLocalizedString = choiceLocalizedStrings[i];
                if (choiceLocalizedString.IsEmpty || choiceLocalizedString == null)
                {
                    Debug.LogError($"[QuizData] LocalizedString is missing for question number: {Number}, choice[{i}]: {choices[i]}");
                    choicesLocalized[i] = choices[i];
                }
                else
                {
                    choicesLocalized[i] = choiceLocalizedString.GetLocalizedString();
                }
            }
            return choicesLocalized;
        }

        /// <summary>
        /// Get question by its localize, but if its localize isn't enable, we will get the default value from CSV instead.
        /// </summary>
        /// <returns></returns>
        public string GetQuestionLocalize() => questionLocalizedString.IsEmpty ? question : questionLocalizedString.GetLocalizedString();

        private string[] ValidateChoices(ref QuizType type, string[] choices)
        {
            switch (type)
            {
                case QuizType.FourChoices:
                    return choices;
                case QuizType.TrueFalse:
                    return choices[..2];
                case QuizType.Sorting:
                    return choices[..3];
                case QuizType.NumberGuessing:
                    return choices[..1];
                default:
                    Debug.LogError($"QuizData: Unsupported quiz type {type}. Setting to None.");
                    type = QuizType.None;
                    return choices;
            }
        }

        private LocalizedString[] ValidateChoicesLocalizedString(ref QuizType type, string[] choices, LocalizedString[] choiceLocalizedStrings)
        {
            var validatedChoices = choiceLocalizedStrings;

            switch (type)
            {
                case QuizType.FourChoices:
                    validatedChoices = choiceLocalizedStrings;
                    break;
                case QuizType.TrueFalse: //True False no localize
                    return null;
                case QuizType.Sorting:
                    validatedChoices = choiceLocalizedStrings[..3];
                    break;
                case QuizType.NumberGuessing: //NumberGuessing no localize
                    return null;
                default:
                    Debug.LogError($"QuizData: Unsupported quiz type {type}. Setting to None.");
                    type = QuizType.None;
                    validatedChoices = choiceLocalizedStrings;
                    break;
            }

            var numberInChoice = 0;
            foreach (var choice in choices)
            {
                if (float.TryParse(choice, out var choiceAsNumber))
                {
                    numberInChoice++;
                }
            }

            if (numberInChoice == validatedChoices.Length)
            {
                //Debug.Log("This choice is number");
                return null;
            }
            else if (numberInChoice > 0)
            {
                Debug.Log($"Make sure the choice number: {Number} has localize of all choices except number choice. Choices: {string.Join(',', choices)}");
            }
            return validatedChoices;
        }
    }
}