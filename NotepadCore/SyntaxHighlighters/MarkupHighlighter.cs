using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using NotepadCore.ExtensionMethods;

namespace NotepadCore.SyntaxHighlighters
{
    public class MarkupHighlighter : IHighlighter
    {
        private static readonly (Regex Pattern, SolidColorBrush Brush)[] Keywords =
        {
            // Uzorak i boja za tagove 
            (new Regex($@"(?<=<\/?)[a-zA-Z][\w:\.]*(?=( |>?)(.|{Environment.NewLine})*?>)"),
                Brushes.Blue),
            // Uzroak i boja za svojstva
            (new Regex(@"(?<= )[a-zA-Z][\w:\.]*(?=="")"), Brushes.Red),
            // Uzorak i boja za vrijednosti svojstva
            (new Regex(@"""(\\""|[^""])*"""), Brushes.Brown)
        };

        private static (Regex Pattern, SolidColorBrush Brush) Comment =>
            (new Regex(@"<!--(.|\n)*?-->"), Brushes.Green);

        IEnumerable<((int Index, int Length) Match, SolidColorBrush Brush)> IHighlighter.GetMatches(TextRange textRange)
        {
            // Vraća indekse svih novih linija u tekstu
            var newLines = textRange.Text.IndexesOf(Environment.NewLine);

            // Petlja koja prolazi kroz sve pogotke za uzorak komentara
            foreach (var (pattern, brush) in Keywords)
            {
                // Petlja koja prolazi kroz sve pogotke za trenutni uzorak 
                foreach (Match match in pattern.Matches(textRange.Text))
                {
                    // Računanje istupa koji je uzrokovan novim linijama
                    int offset = newLines.Count(x => x < match.Index) * Environment.NewLine.Length;
                    yield return ((match.Index - offset, match.Length), brush);
                }
            }
        }

        public IEnumerable<((int Index, int Length) Match, SolidColorBrush Brush)> GetCommentMatches(TextRange textRange)
        {
            // Vraća indekse svih novih linija u tekstu
            var newLines = textRange.Text.IndexesOf(Environment.NewLine).ToList();
            // Petlja koja prolazi kroz sve pogotke za uzorak komentara
            foreach (Match match in Comment.Pattern.Matches(textRange.Text))
            {
                // Računanje istupa koji je uzrokovan novim linijama prije početka pogotka
                int indexOffset = newLines.Count(x => x < match.Index) * Environment.NewLine.Length;
                // Računanje istupa koje je uzrokovan unutar raspona pogotka jer komentari
                // mogu biti u više redova
                int rangeOffset =
                    textRange.Text.Substring(match.Index, match.Length).IndexesOf(Environment.NewLine).Count() *
                    Environment.NewLine.Length;
                yield return ((match.Index - indexOffset, match.Length - rangeOffset), Comment.Brush);
            }
        }
    }
}