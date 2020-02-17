using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;

namespace NotepadCore.SyntaxHighlighters
{
    public interface IHighlighter
    {
        public IEnumerable<(MatchCollection Matches, SolidColorBrush Brush)> GetMatches(TextRange textRange,
            bool multiline = false);
    }
}