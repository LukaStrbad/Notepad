using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;

namespace NotepadCore.SyntaxHighlighters
{
    public class MarkupHighlighter : IHighlighter
    {
        private static readonly (Regex Pattern, SolidColorBrush Brush)[] Keywords =
        {
            
            (new Regex(@"(?<=<\/?)\w+(?=( |>?).*?>)"), Brushes.Blue)// (new Regex(@"<\/?(?<tag>\w+)( |>?).*?>"), Brushes.Blue)
        };

        IEnumerable<((int Index, int Length) Match, SolidColorBrush Brush)> IHighlighter.GetMatches(TextRange textRange,
            bool multiline)
        {
            foreach (var (pattern, brush) in Keywords)
            {
                foreach (Match match in pattern.Matches(textRange.Text))
                {
                    yield return ((match.Index, match.Length), brush);
                }
            }
        }
    }
}