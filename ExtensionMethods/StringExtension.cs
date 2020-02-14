using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotepadCore.ExtensionMethods
{
    public static class StringExtension
    {
        public static List<int> IndexesOf(this string str, string value)
        {
            var indexes = new List<int>();

            if (String.IsNullOrEmpty(value))
                return indexes;

            for (int i = 0; ; i += value.Length)
            {
                i = str.IndexOf(value, i);
                if (i == -1)
                    break;
                indexes.Add(i);
            }

            return indexes;
        }

        /// <summary>
        /// Splits the string at specified indexes
        /// </summary>
        /// <param name="str">This string</param>
        /// <param name="indexes">Array of indexes</param>
        /// <returns>List of strings containing split strings</returns>
        public static List<string> SplitByParams(this string str, params int[] indexes)
        {
            var values = new List<string>(indexes.Length + 1);

            int startIndex = 0;

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
