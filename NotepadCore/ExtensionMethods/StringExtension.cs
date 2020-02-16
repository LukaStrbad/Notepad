using System.Collections.Generic;

namespace NotepadCore.ExtensionMethods
{
    public static class StringExtension
    {
        public static List<int> IndexesOf(this string str, string value)
        {
            var indexes = new List<int>();

            if (string.IsNullOrEmpty(value))
                return indexes;

            for (var i = 0;; i += value.Length)
            {
                i = str.IndexOf(value, i);
                if (i == -1)
                    break;
                indexes.Add(i);
            }

            return indexes;
        }

        /// <summary>
        ///     Splits the string at specified indexes
        /// </summary>
        /// <param name="str">This string</param>
        /// <param name="indexes">Array of indexes</param>
        /// <returns>List of strings containing split strings</returns>
        public static List<string> SplitByParams(this string str, params int[] indexes)
        {
            var values = new List<string>(indexes.Length + 1);

            var startIndex = 0;

            foreach (var index in indexes)
            {
                values.Add(str.Substring(startIndex, index - startIndex));
                startIndex = index;
            }

            values.Add(str.Substring(startIndex));

            return values;
        }
    }
}