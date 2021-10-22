using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SGReader.Core
{
    public class TextHelper
    {
        // Inspired from https://stackoverflow.com/questions/272633/add-spaces-before-capital-letters
        public static string AddSpacesToSentence(string text, bool preserveAcronyms = true)
        {
            bool IsSpaceNeeded(char character)
            {
                return char.IsUpper(character) || char.IsNumber(character);
            }

            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (IsSpaceNeeded(text[i]))
                    if (!char.IsWhiteSpace(text[i - 1]) && !IsSpaceNeeded(text[i - 1]) ||
                        preserveAcronyms && IsSpaceNeeded(text[i - 1]) &&
                        i < text.Length - 1 && !IsSpaceNeeded(text[i + 1]))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        public static string CleanFileName(string filePath)
        {
            // 1 - Get file name withou extension
            var name = Path.GetFileNameWithoutExtension(filePath);
            // 2 - Replace underscore by space
            name = name.Replace('_', ' ');
            // 3 - Add space before capital letters
            name = AddSpacesToSentence(name);
            // 4 - Capitalize first letter of each word
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
        }
    }
}