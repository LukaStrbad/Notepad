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
            "true", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "var", "virtual", "void",
            "volatile"
        };

        private static readonly string[] _keywords2 =
        {
            "case", "catch", "continue", "do", "else", "finally", "for", "foreach", "goto", "if", "switch", "try",
            "while"
        };

        private new static (Regex Pattern, SolidColorBrush Brush)[] Keywords => new []
        {
            (new Regex($@"(?<!\w)({string.Join("|", _keywords1)})(?!\w)"), Brushes.Blue),
            (new Regex($@"(?<!\w)({string.Join("|", _keywords2)})(?!\w)"),
                Brushes.Purple),
            (new Regex(@"""(\\""|[^""])*"""), Brushes.SaddleBrown),
            (new Regex("//.*"), Brushes.Green)
        };

        IEnumerable<(IEnumerable<Group> Matches, SolidColorBrush Brush)> IHighlighter.GetMatches(TextRange textRange,
            bool multiline)
        {
            if (multiline) return GetMultilineMatches(textRange);
            
            var matches = new List<(IEnumerable<Group> Matches, SolidColorBrush Brush)>(Keywords.Length);
        
            foreach (var (pattern, brush) in Keywords)
            {
                matches.Add((pattern.Matches(textRange.Text), brush));
            }
        
            return matches.Where(x => x.Matches.Any());
        }

        IEnumerable<(IEnumerable<Group> Matches, SolidColorBrush Brush)> GetMultilineMatches(TextRange textRange)
        {
            return null;
        }
    }
}