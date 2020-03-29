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
            "object", "operator", "out", "override", "params", "partial", "private", "protected", "public", "readonly", "ref",
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

        private static (Regex Pattern, SolidColorBrush Brush)[] Keywords => new[]
        {
            (new Regex(@"(?<=\.)?[a-zA-Z_]\w*(?=\()"), new BrushConverter().ConvertFromString("#795E26") as SolidColorBrush), // Functions
            (new Regex($@"(?<!\w)({string.Join("|", _keywords1)})(?!\w)"), Brushes.Blue), // Keywords 1
            (new Regex($@"(?<!\w)({string.Join("|", _keywords2)})(?!\w)"), // Keywords 2
                Brushes.Purple),
            (new Regex(@"(\$|@|\$@|@\$)?""(\\""|[^""])*"""), Brushes.Brown), // Strings
            (new Regex(@$"//.*|/\*(.|{Environment.NewLine})*?\*/"), Brushes.Green) // Comments
        };

        IEnumerable<((int Index, int Length) Match, SolidColorBrush Brush)> IHighlighter.GetMatches(TextRange textRange)
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
    }
}