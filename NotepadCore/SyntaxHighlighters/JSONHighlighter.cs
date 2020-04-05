using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;
using NotepadCore.ExtensionMethods;

namespace NotepadCore.SyntaxHighlighters
{
    public class JSONHighlighter : IHighlighter
    {
        private static readonly (Regex Pattern, SolidColorBrush Brush)[] Keywords =
        {
            (new Regex(@""".*?""(?= *:)"),
                Brushes.Blue), // Key
            (new Regex(@"(?<=:) *"".*?"""), Brushes.Brown), // Value string
            (new Regex(@"(?<=:) *(true|false|\d+)"), Brushes.LightSkyBlue) // Value number/bool
        };
        
        private static (Regex Pattern, SolidColorBrush Brush) Comment =>
            (new Regex(@$"//.*|/\*(.|{Environment.NewLine})*?\*/"), Brushes.Green);
        
        public IEnumerable<((int Index, int Length) Match, SolidColorBrush Brush)> GetMatches(TextRange textRange)
        {
            // Gets all indexes of new lines in text
            var newLines = textRange.Text.IndexesOf(Environment.NewLine);

            // Loop through all keywords
            foreach (var (pattern, brush) in Keywords)
            {
                // Loop through all matches for a specific pattern
                foreach (Match match in pattern.Matches(textRange.Text))
                {
                    // Calculate offset because of new lines
                    int offset = newLines.Count(x => x < match.Index) * Environment.NewLine.Length;
                    yield return ((match.Index - offset, match.Length), brush);
                }
            }
        }

        public IEnumerable<((int Index, int Length) Match, SolidColorBrush Brush)> GetCommentMatches(TextRange textRange)
        {
            var newLines = textRange.Text.IndexesOf(Environment.NewLine);

            foreach (Match match in Comment.Pattern.Matches(textRange.Text))
            {
                int indexOffset = newLines.Count(x => x < match.Index) * Environment.NewLine.Length;
                int rangeOffset =
                    textRange.Text.Substring(match.Index, match.Length).IndexesOf(Environment.NewLine).Count() *
                    Environment.NewLine.Length;
                yield return ((match.Index - indexOffset, match.Length - rangeOffset), Comment.Brush);
            }
        }
    }
}