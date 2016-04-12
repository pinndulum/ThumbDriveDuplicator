using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ThumbDriveDuplicator
{
    /// <summary>String extensions for the Mediaport projects</summary>
    public static class StringExtensions
    {
        #region Quotify
        /// <summary>Adds quotes to the instance of the string</summary>
        /// <param name="text">The text to be quotified</param>
        /// <returns>A string that meets the length and escape for each instance of the strings quotes</returns>
        public static string Quotify(this string text)
        {
            if (string.IsNullOrEmpty(text))
                text = string.Empty;
            return text.Quotify(text.Length, null);
        }

        /// <summary>Adds quotes to the instance of the string</summary>
        /// <param name="text">The text to be quotified</param>
        /// <param name="escQuote">The escape to be used to quotify the string</param>
        /// <returns>A string that meets the length and escape for each instance of the strings quotes</returns>
        public static string Quotify(this string text, string escQuote)
        {
            if (string.IsNullOrEmpty(text))
                text = string.Empty;
            return text.Quotify(text.Length, escQuote);
        }

        /// <summary>Adds quotes to the instance of the string and truncates the text to the specified length</summary>
        /// <param name="text">The text to be quotified</param>
        /// <param name="length">The length to trim the string</param>
        /// <returns>A string that meets the length and escape for each instance of the strings quotes</returns>
        public static string Quotify(this string text, int length)
        {
            return text.Quotify(length, null);
        }

        /// <summary>Adds quotes to the instance of the string and truncates the text to the specified length</summary>
        /// <param name="text">The text to be quotified</param>
        /// <param name="length">The length to trim the string</param>
        /// <param name="escQuote">The escape to be used to quotify the string</param>
        /// <returns>A string that meets the length and escape for each instance of the strings quotes</returns>
        public static string Quotify(this string text, int length, string escQuote)
        {
            if (string.IsNullOrEmpty(text))
                return "\"\"";
            if (string.IsNullOrEmpty(escQuote))
                escQuote = "\"\"";
            var regex = new Regex("\"|\r\n|\n");
            var matches = regex.Matches(text).Cast<Match>();
            length += matches.Count(m => m.Value.Equals("\r\n"));
            text = text.Replace("\r\n", " ");
            length += matches.Count(m => m.Value.Equals("\n"));
            text = text.Replace("\n", " ");
            length += matches.Count(m => m.Value.Equals("\"")) * (escQuote.Length - 1);
            text = text.Replace("\"", escQuote);
            text = text.Truncate(length);
            return string.Format("\"{0}\"", text);
        }

        /// <summary>
        /// Creates a formula string by prepending an equal sign to a quotified string.
        /// </summary>
        /// <param name="text">The text to be quotified</param>
        /// <returns>A string that meets the length and escape for each instance of the strings quotes</returns>
        public static string FormulaQuotify(this string text)
        {
            return string.Format("={0}", text.Quotify());
        }

        /// <summary>
        /// Creates a quotified string that can be read from a csv file by Excel.
        /// If the string has a leading zero and all characters are numeric digits, the string is FormualQuotified first.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>A string that meets the length and escape for each instance of the strings quotes</returns>
        public static string CsvQuotify(this string text)
        {
            text = text ?? string.Empty;
            if (text.StartsWith("0") && text.ToCharArray().All(c => char.IsDigit(c)))
                return text.FormulaQuotify().Quotify();
            return text.Quotify();
        }
        #endregion

        #region Truncate
        /// <summary>Truncates the specified string to the length specified</summary>
        /// <param name="text">The text to be truncated</param>
        /// <param name="trimLength">The length of the string to be truncated to</param>
        /// <returns>A string representation of the string after being converted</returns>
        public static string Truncate(this string text, int trimLength) { return Truncate(text, trimLength, false, false); }

        /// <summary>Truncates the specified string to the length specified</summary>
        /// <param name="text">The text to be truncated</param>
        /// <param name="trimLength">The length of the string to be truncated to</param>
        /// <param name="addEllipses">Includes an ellipses after the string</param>
        /// <returns>A string representation of the string after being converted</returns>
        public static string Truncate(this string text, int trimLength, bool addEllipses) { return Truncate(text, trimLength, false, false); }

        /// <summary>Truncates the specified string to the length specified</summary>
        /// <param name="text">The text to be truncated</param>
        /// <param name="trimLength">The length of the string to be truncated to</param>
        /// <param name="addEllipses">Includes an ellipses after the string</param>
        /// <param name="useEllipsesChar">Use the ellipses character instead of three seperate period chars</param>
        /// <returns>A string representation of the string after being converted</returns>
        public static string Truncate(this string text, int trimLength, bool addEllipses, bool useEllipsesChar)
        {
            string ellipsesChars = addEllipses ? useEllipsesChar ? "…" : "..." : string.Empty;
            // Excel formula
            // =IF(LEN(A2) <= 18, A2, IF(LEN(A2) > 18, CONCATENATE(LEFT(A2, 15), "…"),LEFT(A2, 18)))
            var value = text ?? string.Empty;
            if (string.IsNullOrEmpty(value)) return value;
            value = value.Trim();
            var valueLength = value.Length;

            if (valueLength <= trimLength) return value;

            var l = addEllipses ? trimLength - 3 : trimLength;
            return value.Substring(0, l) + (ellipsesChars);
        }
        #endregion

        #region GetLines
        /// <summary>Seperates the specified string into an enumerable of strings based on a new line character</summary>
        /// <param name="text">The text to be seperated</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> GetLines(this string text) { return text.GetLines(false); }

        /// <summary>Seperates the specified string into an enumerable of strings based on a new line character</summary>
        /// <param name="text">The text to be seperated</param>
        /// <param name="removeEmptyEntries">Removes the empty strings from the enumerable</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> GetLines(this string text, bool removeEmptyEntries) { return text.Separate(removeEmptyEntries, Environment.NewLine); }
        #endregion

        #region Seperate
        /// <summary>Seperates the specified string into an enumerable of strings based on the given seperators</summary>
        /// <param name="text">The text to be seperated</param>
        /// <param name="separators">The seperators to split the string on</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> Separate(this string text, IEnumerable<char> separators) { return text.Separate(false, separators); }

        /// <summary>Seperates the specified string into an enumerable of strings based on the given seperators</summary>
        /// <param name="text">The text to be seperated</param>
        /// <param name="count">The maximum number of strings to be returned</param>
        /// <param name="separators">The seperators to split the string on</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> Separate(this string text, int count, IEnumerable<char> separators) { return text.Separate(count, false, separators); }

        /// <summary>Seperates the specified string into an enumerable of strings based on the given seperators</summary>
        /// <param name="text">The text to be seperated</param>
        /// <param name="removeEmptyEntries">Removes the empty strings from the enumerable</param>
        /// <param name="separators">The seperators to split the string on</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> Separate(this string text, bool removeEmptyEntries, IEnumerable<char> separators) { return text.Separate(removeEmptyEntries, separators.Select(c => c.ToString()).ToArray()); }

        /// <summary>Seperates the specified string into an enumerable of strings based on the given seperators</summary>
        /// <param name="text">The text to be seperated</param>
        /// <param name="count">The maximum number of strings to be returned</param>
        /// <param name="removeEmptyEntries">Removes the empty strings from the enumerable</param>
        /// <param name="separators">The seperators to split the string on</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> Separate(this string text, int count, bool removeEmptyEntries, IEnumerable<char> separators) { return text.Separate(count, removeEmptyEntries, separators.Select(c => c.ToString()).ToArray()); }

        /// <summary>Seperates the specified string into an enumerable of strings based on the given seperators</summary>
        /// <param name="text">The text to be seperated</param>
        /// <param name="separators">The seperators to split the string on</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> Separate(this string text, params char[] separators) { return text.Separate(false, separators); }

        /// <summary>Seperates the specified string into an enumerable of strings based on the given seperators</summary>
        /// <param name="text">The text to be seperated</param>
        /// <param name="count">The maximum number of strings to be returned</param>
        /// <param name="separators">The seperators to split the string on</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> Separate(this string text, int count, params char[] separators) { return text.Separate(count, false, separators); }

        /// <summary>Seperates the specified string into an enumerable of strings based on the given seperators</summary>
        /// <param name="text">The text to be seperated</param>
        /// <param name="removeEmptyEntries">Removes the empty strings from the enumerable</param>
        /// <param name="separators">The seperators to split the string on</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> Separate(this string text, bool removeEmptyEntries, params char[] separators) { return text.Separate(removeEmptyEntries, separators.Select(c => c.ToString()).ToArray()); }

        /// <summary>Seperates the specified string into an enumerable of strings based on the given seperators</summary>
        /// <param name="text">The text to be seperated</param>
        /// <param name="count">The maximum number of strings to be returned</param>
        /// <param name="removeEmptyEntries">Removes the empty strings from the enumerable</param>
        /// <param name="separators">The seperators to split the string on</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> Separate(this string text, int count, bool removeEmptyEntries, params char[] separators) { return text.Separate(count, removeEmptyEntries, separators.Select(c => c.ToString()).ToArray()); }

        /// <summary>Seperates the specified string into an enumerable of strings based on the given seperators</summary>
        /// <param name="text">The text to be seperated</param>
        /// <param name="separators">The seperators to split the string on</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> Separate(this string text, IEnumerable<string> separators) { return text.Separate(false, separators); }

        /// <summary>Seperates the specified string into an enumerable of strings based on the given seperators</summary>
        /// <param name="text">The text to be seperated</param>
        /// <param name="count">The maximum number of strings to be returned</param>
        /// <param name="separators">The seperators to split the string on</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> Separate(this string text, int count, IEnumerable<string> separators) { return text.Separate(count, false, separators); }

        /// <summary>Seperates the specified string into an enumerable of strings based on the given seperators</summary>
        /// <param name="text">The text to be seperated</param>
        /// <param name="removeEmptyEntries">Removes the empty strings from the enumerable</param>
        /// <param name="separators">The seperators to split the string on</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> Separate(this string text, bool removeEmptyEntries, IEnumerable<string> separators) { return text.Separate(removeEmptyEntries, separators.ToArray()); }

        /// <summary>Seperates the specified string into an enumerable of strings based on the given seperators</summary>
        /// <param name="text">The text to be seperated</param>
        /// <param name="count">The maximum number of strings to be returned</param>
        /// <param name="removeEmptyEntries">Removes the empty strings from the enumerable</param>
        /// <param name="separators">The seperators to split the string on</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> Separate(this string text, int count, bool removeEmptyEntries, IEnumerable<string> separators) { return text.Separate(count, removeEmptyEntries, separators.ToArray()); }

        /// <summary>Seperates the specified string into an enumerable of strings based on the given seperators</summary>
        /// <param name="text">The text to be seperated</param>
        /// <param name="separators">The seperators to split the string on</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> Separate(this string text, params string[] separators) { return text.Separate(false, separators); }

        /// <summary>Seperates the specified string into an enumerable of strings based on the given seperators</summary>
        /// <param name="text">The text to be seperated</param>
        /// <param name="count">The maximum number of strings to be returned</param>
        /// <param name="separators">The seperators to split the string on</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> Separate(this string text, int count, params string[] separators) { return text.Separate(count, false, separators); }

        /// <summary>Seperates the specified string into an enumerable of strings based on the given seperators</summary>
        /// <param name="text">The text to be seperated</param>
        /// <param name="removeEmptyEntries">Removes the empty strings from the enumerable</param>
        /// <param name="separators">The seperators to split the string on</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> Separate(this string text, bool removeEmptyEntries, params string[] separators)
        {
            if (string.IsNullOrEmpty(text))
                yield break;
            foreach (var part in text.Split(separators, removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None))
                yield return part;
        }

        /// <summary>Seperates the specified string into an enumerable of strings based on the given seperators</summary>
        /// <param name="text">The text to be seperated</param>
        /// <param name="count">The maximum number of strings to be returned</param>
        /// <param name="removeEmptyEntries">Removes the empty strings from the enumerable</param>
        /// <param name="separators">The seperators to split the string on</param>
        /// <returns>An enumerable of strings after the seperation</returns>
        public static IEnumerable<string> Separate(this string text, int count, bool removeEmptyEntries, params string[] separators)
        {
            if (string.IsNullOrEmpty(text))
                yield break;
            foreach (var part in text.Split(separators, count, removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None))
                yield return part;
        }
        #endregion

        #region Delimited enumberables
        /// <summary>Converts an enumberable object to a comma delimited string representation of the objects</summary>
        /// <typeparam name="T">The type of enumerable to be converted</typeparam>
        /// <param name="items">The items to be converted to a comma delimited string</param>
        /// <returns>A string representation of the enumberable as a comma delimited string</returns>
        public static string MakeCommaDelimited<T>(this IEnumerable<T> items)
        {
            return items.MakeDelimited<T>(", ");
        }

        /// <summary>Converts an enumberable object to a <typeparamref name="separator"/> delimited string representation of the objects</summary>
        /// <typeparam name="T">The type of enumerable to be converted</typeparam>
        /// <param name="items">The items to be converted to a delimited string</param>
        /// <param name="separator">The seperator used to join the object together</param>
        /// <returns>A string representation of the enumberable as a delimited string</returns>
        public static string MakeDelimited<T>(this IEnumerable<T> items, string separator)
        {
            return string.Join(separator, items.Select(i => i.ToString()).ToArray());
        }
        #endregion

        #region Other
        /// <summary>Converts the specified string to Title Case</summary>
        /// <param name="str">The string to be converted to title case</param>
        /// <returns>A string representation of the string after being converted</returns>
        public static string ToTitleCase(this string str) { return System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str); }

        /// <summary>
        /// Formats a message using the same basic idea as a String.Format but instead of using the {0} it uses the
        /// properties of a class and replaces {PropertyName} with the value of that property if present
        /// </summary>
        /// <param name="message">The message that is to be formatted</param>
        /// <param name="values">
        /// The values in a new or existing object that are to replace the variables in the message (matching
        /// properties to variables in the message)
        /// </param>
        /// <returns>A formatted string with the variables replaced with actual values</returns>
        public static string FormatMessage(this string message, object values)
        {
            return values.FormatString(message);
        }

        public static bool ParseKeyValue(this string text, out string key, out string value, bool trim)
        {
            var b = text.ParseKeyValue<string>(out key, out value);
            key = trim && !string.IsNullOrEmpty(key) ? key.Trim() : key;
            value = trim && !string.IsNullOrEmpty(value) ? value.Trim() : value;
            return b;
        }

        public static bool ParseKeyValue<T>(this string text, out string key, out T value)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            var index = text.IndexOf('=');
            if (index == -1)
            {
                key = text;
                value = default(T);
                return false;
            }
            while (index < text.Length - 1 && text[index + 1] == '=')
                index = text.IndexOf('=', index + 2);
            if (index == -1)
            {
                key = text.Replace("==", "=");
                value = default(T);
                return false;
            }
            key = text.Substring(0, index).Replace("==", "=");
            value = text.Substring(index + 1).ParseOrDefault<T>();
            return true;
        }
        #endregion
    }
}
