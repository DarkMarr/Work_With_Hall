using System.Text.RegularExpressions;

namespace QuizGame.Utilities
{
    public static class StringUtilities
    {
        public static string SplitCamelCase(string input)
        {
            return Regex.Replace(input, "(\\B[A-Z])", " $1");
        }
    }
}
