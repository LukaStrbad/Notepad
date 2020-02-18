using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using NotepadCore.ExtensionMethods;
using NotepadCore.SyntaxHighlighters;

namespace NotepadCore.Settings
{
    /// <summary>
    ///     A class that stores user settings
    /// </summary>
    public sealed class Settings
    {
        private static readonly string SavePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\settings.xml");

        private static readonly Settings DefaultSettings = new Settings
        {
            Editors = new[] {new EditorInfo() },
            EditorFontFamily = "Consolas",
            EditorFontSize = 12,
            TabSize = 4,
            SelectedFileIndex = 0,
            ShowLineNumbers = true,
            UseSpaces = true
        };

        private string _editorFontFamily;
        private int _editorFontSize;
        private EditorInfo[] _editors;

        private int _selectedFileIndex;
        private int _tabSize;

        private Settings()
        {
        }

        public EditorInfo[] Editors
        {
            get => _editors.Distinct().ToArray();
            set => _editors = value.Distinct().ToArray();
        }

        public string EditorFontFamily
        {
            get
            {
                try
                {
                    new FontFamily(_editorFontFamily);
                    return _editorFontFamily;
                }
                catch
                {
                    _editorFontFamily = DefaultSettings.EditorFontFamily;
                }

                return _editorFontFamily;
            }
            set
            {
                // sets the editor font family if it's not null
                try
                {
                    new FontFamily(value);
                    _editorFontFamily = value;
                }
                catch
                {
                    _editorFontFamily = "Consolas";
                    return;
                }

                _editorFontFamily = value;
            }
        }

        public int EditorFontSize
        {
            get
            {
                if (!(_editorFontSize >= 8 && _editorFontSize <= 96))
                    _editorFontSize = DefaultSettings.EditorFontSize;

                return _editorFontSize;
            }
            set
            {
                if (value >= 8 && value <= 96)
                    _editorFontSize = value;
                else
                    _editorFontSize = DefaultSettings.EditorFontSize;
            }
        }

        /// <summary>
        ///     Gets or sets a custom tab size
        /// </summary>
        public int TabSize
        {
            get
            {
                if (_tabSize <= 0)
                    _tabSize = DefaultSettings.TabSize;

                return _tabSize;
            }
            set
            {
                if (value > 0)
                    _tabSize = value;
                else
                    _tabSize = DefaultSettings.TabSize;
            }
        }

        /// <summary>
        ///     Gets saved file paths
        /// </summary>
        // public string[] FilePaths
        // {
        //     get { return _filePaths.Select(x => x.ToLower()).Distinct().ToArray() ?? DefaultUserSettings.FilePaths; }
        //     set => _filePaths = value.Select(x => x.ToLower()).Distinct().ToArray() ?? DefaultUserSettings.FilePaths;
        // }

        public int SelectedFileIndex
        {
            get
            {
                if (_selectedFileIndex >= 0 && _selectedFileIndex < Editors.Length)
                    return _selectedFileIndex;

                _selectedFileIndex = DefaultSettings.SelectedFileIndex;
                return _selectedFileIndex;
            }
            set
            {
                if (_selectedFileIndex >= 0 && _selectedFileIndex < Editors.Length)
                    _selectedFileIndex = value;
                else
                    _selectedFileIndex = DefaultSettings.SelectedFileIndex;
            }
        }

        public bool ShowLineNumbers { get; set; } = true;

        public bool UseSpaces { get; set; } = true;

        /// <summary>
        ///     Removes all occurances of the path
        /// </summary>
        /// <param name="path">Path to remove</param>
        private void RemoveFilePath(string path)
        {
            // if there are multiple occurrences
            while (Editors.Select(x => x.FilePath).Contains(path))
                Editors = Editors.Where(x => x.FilePath != path).ToArray();
        }

        /// <summary>
        ///     Removes all occurrences of paths in the array
        /// </summary>
        /// <param name="paths">Paths to remove</param>
        public void RemoveFilePaths(params string[] paths)
        {
            foreach (var path in paths)
                RemoveFilePath(path);
        }

        /// <summary>
        ///     Adds distinct file paths to FilePaths
        /// </summary>
        /// <param name="paths"></param>
        public void AddFiles(params string[] paths)
        {
            // adds distinct paths to FilePaths setting
            Editors = new[] {Editors, paths.Select(x => new EditorInfo(HighlightingLanguage.None, x)).ToArray()}.SelectMany(x => x).Distinct().ToArray();
        }

        public void Save()
        {
            using (var streamWriter = new StreamWriter(SavePath, false))
            {
                var serializer = new XmlSerializer(typeof(Settings));
                serializer.Serialize(streamWriter, this);
            }
        }

        public static Settings Create()
        {
            var serializer = new XmlSerializer(typeof(Settings));

            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            try
            {
                using (var streamReader = new StreamReader(SavePath))
                {
                    var temp = (Settings) serializer.Deserialize(streamReader);
                    return temp;
                }
            }
            catch
            {
                using (var streamWriter = new StreamWriter(SavePath))
                {
                    serializer.Serialize(streamWriter, DefaultSettings);
                }
            }

            return DefaultSettings;
        }
    }
}