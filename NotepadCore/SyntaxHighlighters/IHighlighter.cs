using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;

namespace NotepadCore.SyntaxHighlighters
{
    public interface IHighlighter
    {
        /// <summary>
        /// Returns all matches
        /// </summary>
        /// <param name="textRange"></param>
        /// <returns></returns>
        IEnumerable<((int Index, int Length) Match, SolidColorBrush Brush)> GetMatches(TextRange textRange);
    }
}