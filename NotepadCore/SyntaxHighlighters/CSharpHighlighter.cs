using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace NotepadCore.SyntaxHighlighters
{
    public class CSharpHighlighter : IHighlighter
    {
        private static readonly string[] _keywords1 =
        {
            "abstract", "as", "base", "bool", "break", "byte", "char", "checked", "class", "const", "decimal",
            "default", "delegate", "double", "enum", "event", "explicit", "extern", "false", "fixed", "float",
            "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null",
            "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref",
            "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "this",
            "throw",
            "true", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void",
            "volatile"
        };

        private static readonly string[] _keywords2 =
        {
            "case", "catch", "continue", "do", "else", "finally", "for", "foreach", "goto", "if", "switch", "try",
            "while"
        };

        private static readonly (Regex Pattern, SolidColorBrush Brush)[] Keywords =
        {
            // var pattern = new Regex($@"(?<!\w)({string.Join("|", tuple.Keywords)})(?!\w)");
            (new Regex($@"(?<!\w)({string.Join("|", _keywords1)})(?!\w)"), Brushes.Blue),
            (new Regex($@"(?<!\w)({string.Join("|", _keywords2)})(?!\w)"),
                Brushes.Purple),
            (new Regex(@"""(\\""|[^""])*"""), Brushes.SaddleBrown),
            (new Regex("//.*"), Brushes.Green)
        };

        public static IEnumerable<(MatchCollection Matches, SolidColorBrush Brush)> GetMatches(TextRange textRange,
            bool multiline = false)
        {
            var matches = new List<(MatchCollection Matches, SolidColorBrush Brush)>(Keywords.Length);

            foreach (var (pattern, brush) in Keywords)
            {
                matches.Add((pattern.Matches(textRange.Text), brush));
            }

            return matches.Where(x => x.Matches.Count > 0);
        }

        IEnumerable<(MatchCollection Matches, SolidColorBrush Brush)> IHighlighter.GetMatches(TextRange textRange,
            bool multiline)
        {
            return GetMatches(textRange, multiline);
        }
    }
}