using NotepadCore.SyntaxHighlighters;

namespace NotepadCore.Settings
{
    public class EditorInfo
    {
        public EditorInfo()
        {
            HighlightingLanguage = HighlightingLanguage.None;
            FilePath = "";
        }

        public EditorInfo(HighlightingLanguage highlightingLanguage, string filePath)
        {
            HighlightingLanguage = highlightingLanguage;
            FilePath = filePath;
        }

        public HighlightingLanguage HighlightingLanguage { get; set; }
        public string FilePath { get; set; }
    }
}