using System.Globalization;
using System.IO;
using System.Text;

namespace SGReader.Core
{
    public static class TextHelper
    {
        public static string CleanFileName(string filePath)
        {
            var name = Path.GetFileNameWithoutExtension(filePath)?.Replace('_', ' ');
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(AddSpacesBeforeCaps(name));
        }

        private static string AddSpacesBeforeCaps(string text)
        {
            var result = new StringBuilder(text.Length * 2);
            result.Append(text[0]);

            for (int i = 1; i < text.Length; i++)
            {
                char current = text[i];
                char previous = text[i - 1];
                bool currentBreak = char.IsUpper(current) || char.IsNumber(current);
                bool previousBreak = char.IsUpper(previous) || char.IsNumber(previous);

                if (currentBreak
                    && !char.IsWhiteSpace(previous)
                    && (!previousBreak
                        || (i < text.Length - 1 && !char.IsUpper(text[i + 1]) && !char.IsNumber(text[i + 1]))))
                {
                    result.Append(' ');
                }

                result.Append(current);
            }

            return result.ToString();
        }
    }
}
