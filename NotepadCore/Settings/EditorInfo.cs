using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NotepadCore.Settings
{
    public class EditorInfo:  IEquatable<EditorInfo>, IEquatable<string>
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


        public bool Equals(EditorInfo other)
        {
            return FilePath == other?.FilePath;
        }

        public bool Equals(string other)
        {
            return FilePath == other;
        }
    }
}