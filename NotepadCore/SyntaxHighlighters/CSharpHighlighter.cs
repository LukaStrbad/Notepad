using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using NotepadCore.ExtensionMethods;

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
            "true", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "var", "virtual", "void",
            "volatile"
        };

        private static readonly string[] _keywords2 =
        {
            "case", "catch", "continue", "do", "else", "finally", "for", "foreach", "goto", "if", "switch", "try",
            "while"
        };

        private new static (Regex Pattern, SolidColorBrush Brush)[] Keywords => new[]
        {
            (new Regex($@"(?<!\w)({string.Join("|", _keywords1)})(?!\w)"), Brushes.Blue),
            (new Regex($@"(?<!\w)({string.Join("|", _keywords2)})(?!\w)"),
                Brushes.Purple),
            (new Regex(@"""(\\""|[^""])*"""), Brushes.SaddleBrown),
            (new Regex(@$"//.*|/\*(.|{Environment.NewLine})*?\*/"), Brushes.Green)
        };

        IEnumerable<((int Index, int Length) Match, SolidColorBrush Brush)> IHighlighter.GetMatches(TextRange textRange,
            bool multiline)
        {
            var indexes = textRange.Text.IndexesOf(Environment.NewLine);

            foreach (var (pattern, brush) in Keywords)
            {
                foreach (Match match in pattern.Matches(textRange.Text))
                {
                    int offset = indexes.Count(x => x < match.Index) * Environment.NewLine.Length;
                    yield return ((match.Index - offset, match.Length), brush);
                }
            }
        }
    }
}