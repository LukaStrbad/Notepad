using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using NotepadCore.ExtensionMethods;

namespace NotepadCore.SyntaxHighlighters
{
    public class CSharpHighlighter : IHighlighter
    {
        // Popis ključnih riječi koje se označuju jednom bojom
        private static readonly string[] _keywords1 =
        {
            "abstract", "as", "base", "bool", "break", "byte", "char", "checked", "class", "const", "decimal",
            "default", "delegate", "double", "enum", "event", "explicit", "extern", "false", "fixed", "float",
            "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null",
            "object", "operator", "out", "override", "params", "partial", "private", "protected", "public", "readonly",
            "ref",
            "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "this",
            "throw",
            "true", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "var", "virtual", "void",
            "volatile"
        };

        // Popis ključnih riječi koje se označuju drugom bojom
        private static readonly string[] _keywords2 =
        {
            "case", "catch", "continue", "do", "else", "finally", "for", "foreach", "get", "goto", "if", "set",
            "switch", "try", "while"
        };

        private static (Regex Pattern, SolidColorBrush Brush)[] Keywords => new[]
        {
            // Uzorak i boja za funkcije
            (new Regex(@"(?<=\.)?[a-zA-Z_]\w*(?=\()"),
                new BrushConverter().ConvertFromString("#795E26") as SolidColorBrush),
            // Uzorak i boja za prve ključne riječi
            (new Regex($@"(?<!\w)({string.Join("|", _keywords1)})(?!\w)"), Brushes.Blue),
            // Uzorak i boja za druge ključne riječi
            (new Regex($@"(?<!\w)({string.Join("|", _keywords2)})(?!\w)"),
                Brushes.Purple),
            // Uzorak i boja za isticanje stringova
            (new Regex(@"(\$|@|\$@|@\$)?""(\\""|[^""])*""|'(\\'|[^'])*?'"), Brushes.Brown)
        };

        private static (Regex Pattern, SolidColorBrush Brush) Comment =>
            (new Regex(@$"//.*|/\*(.|{Environment.NewLine})*?\*/"), Brushes.Green);

        IEnumerable<((int Index, int Length) Match, SolidColorBrush Brush)> IHighlighter.GetMatches(TextRange textRange)
        {
            // Vraća indekse svih novih linija u tekstu 
            var newLines = textRange.Text.IndexesOf(Environment.NewLine);

            // Petlja koja prolazi kroz sve ključne riječi
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

        public IEnumerable<((int Index, int Length) Match, SolidColorBrush Brush)> GetCommentMatches(
            TextRange textRange)
        {
            // Vraća indekse svih novih linija u tekstu
            var newLines = textRange.Text.IndexesOf(Environment.NewLine);
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