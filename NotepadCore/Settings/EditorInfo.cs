using NotepadCore.SyntaxHighlighters;

namespace NotepadCore.Settings
{
    public class EditorInfo
    {
        // Konstruktor bez argumenata
        public EditorInfo()
        {
            // Zadani jezik isticanja je None
            HighlightingLanguage = HighlightingLanguage.None;
            // Zadana putanja pohrane je prazna
            FilePath = "";
        }
        // Konstruktor sa argumentima za jezik isticanja i putanju do datetke
        public EditorInfo(HighlightingLanguage highlightingLanguage, string filePath)
        {
            // Spremanje argumenata u svojstva
            HighlightingLanguage = highlightingLanguage;
            FilePath = filePath;
        }

        // Svojstvo za jezik isticanja
        public HighlightingLanguage HighlightingLanguage { get; set; }
        // Svojstvo za putanju spremanja
        public string FilePath { get; set; }
    }
}