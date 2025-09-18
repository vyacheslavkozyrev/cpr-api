using System.Text.RegularExpressions;

namespace CPR.Application.Services
{
    /// <summary>
    /// Utility class for input sanitization
    /// </summary>
    public static class InputSanitizer
    {
        private static readonly Regex HtmlTagRegex = new Regex(@"<[^>]*>", RegexOptions.Compiled);
        private static readonly Regex ScriptTagRegex = new Regex(@"<script[^>]*>.*?</script>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SqlInjectionRegex = new Regex(@"(\b(union|select|insert|delete|update|drop|create|alter|exec|execute)\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Sanitizes text input by removing HTML tags and potentially dangerous content
        /// </summary>
        /// <param name="input">The input text to sanitize</param>
        /// <returns>Sanitized text</returns>
        public static string SanitizeText(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Remove script tags
            input = ScriptTagRegex.Replace(input, "");

            // Remove HTML tags
            input = HtmlTagRegex.Replace(input, "");

            // Basic SQL injection prevention (remove suspicious keywords)
            input = SqlInjectionRegex.Replace(input, "[FILTERED]");

            // Trim whitespace
            return input.Trim();
        }

        /// <summary>
        /// Validates that the content doesn't contain excessive special characters
        /// </summary>
        /// <param name="content">The content to validate</param>
        /// <returns>True if content is valid</returns>
        public static bool IsValidContent(string content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            if (string.IsNullOrWhiteSpace(content))
                return false;

            // Check length (max 1000 characters)
            if (content.Length > 1000)
                return false;

            // Check for excessive special characters (more than 50% special chars)
            var specialCharCount = content.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
            var specialCharRatio = (double)specialCharCount / content.Length;

            return specialCharRatio <= 0.5;
        }
    }
}