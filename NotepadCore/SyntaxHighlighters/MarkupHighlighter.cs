using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using NotepadCore.ExtensionMethods;
using NotepadCore.Types;

namespace NotepadCore.SyntaxHighlighters
{
    public class MarkupHighlighter : IHighlighter
    {
        private static readonly (Regex Pattern, SolidColorBrush Brush)[] Keywords =
        {
            (new Regex(@"(?<=<\/?)[a-zA-Z][\w:\.]*(?=( |>?)(.|(\n|\r|\r\n))*?>)", RegexOptions.Multiline),
                Brushes.Blue), // Tags
            (new Regex(@"(?<= )[a-zA-Z][\w:\.]*(?=="")"), Brushes.Red), // Properties
            (new Regex(@"""(\\""|[^""])*"""), Brushes.Brown), // Strings,
            (new Regex(@"<!--(.|\n)*?-->"), Brushes.Green) // Comments
        };

        IEnumerable<((int Index, int Length) Match, SolidColorBrush Brush)> IHighlighter.GetMatches(TextRange textRange)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var newLines = textRange.Text.IndexesOf(Environment.NewLine).ToArray();
            var matches =
                new SortedCollection<((int Index, int Length) Match, SolidColorBrush Brush)>(x => x.Match.Index);

            foreach (var (pattern, brush) in Keywords)
            {
                foreach (Match match in pattern.Matches(textRange.Text))
                {
                    matches.Add(((match.Index, match.Length), brush));
                }
            }


            Debug.Write($"Time 1: {sw.ElapsedMilliseconds}");

            int currentLine = 0;

            foreach (var (match, brush) in matches)
            {
                if ( currentLine < newLines.Length && match.Index >= newLines[currentLine])
                    currentLine++;
                yield return ((match.Index - currentLine * Environment.NewLine.Length, match.Length), brush);
            }

            Debug.WriteLine($"Time 2: {sw.ElapsedMilliseconds}");
        }
    }
}