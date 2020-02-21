using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;
using NotepadCore.ExtensionMethods;

namespace NotepadCore.SyntaxHighlighters
{
    public class MarkupHighlighter : IHighlighter
    {
        private static readonly (Regex Pattern, SolidColorBrush Brush)[] Keywords =
        {
            
            (new Regex(@"(?<=<\/?)\w+(?=( |>?).*?>)"), Brushes.Blue),
            (new Regex(@"<!--(.|\n)*?-->"), Brushes.Green)
        };

        IEnumerable<((int Index, int Length) Match, SolidColorBrush Brush)> IHighlighter.GetMatches(TextRange textRange)
        {
            var newLines = textRange.Text.IndexesOf(Environment.NewLine);
            
            foreach (var (pattern, brush) in Keywords)
            {
                foreach (Match match in pattern.Matches(textRange.Text))
                {
                    int offset = newLines.Count(x => x < match.Index) * Environment.NewLine.Length;
                    yield return ((match.Index - offset, match.Length), brush);
                }
            }
        }
    }
}