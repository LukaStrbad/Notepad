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
            (new Regex(@"<\/?(?<tag>\w+)( |>?).*?>"), Brushes.Blue)
        };

        public IEnumerable<(MatchCollection Matches, SolidColorBrush Brush)> GetMatches(TextRange textRange,
            bool multiline = false)
        {
            var matches = new List<(MatchCollection Matches, SolidColorBrush Brush)>(Keywords.Length);

            foreach (var (pattern, brush) in Keywords)
            {
                matches.Add((pattern.Matches(textRange.Text), brush));
            }

            return matches.Where(x => x.Matches.Count > 0);
        }
    }
}