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

        IEnumerable<(IEnumerable<Group> Matches, SolidColorBrush Brush)> IHighlighter.GetMatches(TextRange textRange,
            bool multiline)
        {
            var matches = new List<(IEnumerable<Group> Matches, SolidColorBrush Brush)>(Keywords.Length);

            foreach (var (pattern, brush) in Keywords)
            {
                matches.Add((pattern.Matches(textRange.Text), brush));
            }

            return matches.Where(x => x.Matches.Any());
        }
    }
}