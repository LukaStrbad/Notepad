using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;

namespace NotepadCore.SyntaxHighlighters
{
    public interface IHighlighter
    {
        // Metoda vraća sve normalne pogotke sa pruženom informacijom o indeksu 
        // pogotka, duljini pogotka i boji isticanja
        IEnumerable<((int Index, int Length) Match, SolidColorBrush Brush)> GetMatches(TextRange textRange);

        // Metoda vraća informacije o pogotku za komentare
        IEnumerable<((int Index, int Length) Match, SolidColorBrush Brush)> GetCommentMatches(TextRange textRange);
    }
}