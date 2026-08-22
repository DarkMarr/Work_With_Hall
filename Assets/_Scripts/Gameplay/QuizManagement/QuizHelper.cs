using System;
using System.Collections.Generic;
using QuizGame.Destination;

namespace QuizGame.Gameplay.QuizManagement
{
    public class QuizHelper
    {
        public static readonly Dictionary<string, QuizType> TypeMapping = new Dictionary<string, QuizType>
        {
            { "4 Choice", QuizType.FourChoices },
            { "True False", QuizType.TrueFalse },
            { "Sorting", QuizType.Sorting },
            { "Number Guessing", QuizType.NumberGuessing }
        };

        public static readonly Dictionary<string, QuizCategory> CategoryMapping = new Dictionary<string, QuizCategory>
        {
            { "General", QuizCategory.General },
            { "Geography", QuizCategory.Geography },
            { "History", QuizCategory.History },
            { "Pop Culture", QuizCategory.PopCulture },
            { "Entertainment", QuizCategory.Entertainment },
            { "Science", QuizCategory.Science },
            { "sports", QuizCategory.Sports },
            { "none", QuizCategory.None }
        };

        public static readonly Dictionary<string, DestinationType> DestinationMapping = new Dictionary<string, DestinationType>
        {
            { "#Bangkok", DestinationType.Bangkok },
            { "#Tokyo", DestinationType.Tokyo },
            { "#New York", DestinationType.NewYork },
            { "#Paris", DestinationType.NewYork },
            { "#London", DestinationType.London },
            { "#Cairo", DestinationType.Cairo },
            { "#none", DestinationType.None }
        };

        public static QuizCategory GetAllQuizCategory()
        {
            QuizCategory all = 0;
            foreach (QuizCategory category in Enum.GetValues(typeof(QuizCategory)))
            {
                all |= category;
            }
            return all;
        }

        public static List<QuizCategory> GetAllQuizCategoryList()
        {
            return new List<QuizCategory>
            {
                QuizCategory.General,
                QuizCategory.Geography,
                QuizCategory.History,
                QuizCategory.PopCulture,
                QuizCategory.Entertainment,
                QuizCategory.Science,
                QuizCategory.Sports
            };
        }
    }

    public enum QuizType
    {
        FourChoices,
        TrueFalse,
        Sorting,
        NumberGuessing,
        None
    }

    public enum QuizDifficultyLevel
    {
        Easy = 1,
        Medium = 2,
        Hard = 3
    }

    [Flags]
    public enum QuizCategory
    {
        General = 1 << 0,
        Geography = 1 << 1,
        History = 1 << 2,
        PopCulture = 1 << 3,
        Entertainment = 1 << 4,
        Science = 1 << 5,
        Sports = 1 << 6,
        None = 0
    }
}
