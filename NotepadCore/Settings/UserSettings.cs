using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using NotepadCore.ExtensionMethods;
using NotepadCore.SyntaxHighlighters;

namespace NotepadCore.Settings
{
    /// <summary>
    ///     A class that manages more complex user settings
    /// </summary>
    public sealed class UserSettings
    {
        private static readonly string SavePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\settings.xml");

        private static readonly UserSettings DefaultUserSettings = new UserSettings
        {
            Editors = new EditorInfo[] { new EditorInfo(),  },
            EditorFontFamily = "Consolas",
            EditorFontSize = 12,
            TabSize = 4,
            SelectedFileIndex = 0
        };

        private string _editorFontFamily;
        private int _editorFontSize;
        private EditorInfo[] _editors;

        private int _selectedFileIndex;
        private int _tabSize;

        private UserSettings()
        {
        }

        public EditorInfo[] Editors
        {
            get
            {
                if (_editors == null)
                    _editors = new EditorInfo[] { };
                return _editors.Distinct(editor => editor.FilePath.ToLower()).ToArray();
            }
            set => _editors = value?.Distinct(editor => editor.FilePath.ToLower()).ToArray() ?? new[]{new EditorInfo(), };
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
                    _editorFontFamily = DefaultUserSettings.EditorFontFamily;
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
                    _editorFontSize = DefaultUserSettings.EditorFontSize;

                return _editorFontSize;
            }
            set
            {
                if (value >= 8 && value <= 96)
                    _editorFontSize = value;
                else
                    _editorFontSize = DefaultUserSettings.EditorFontSize;
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
                    _tabSize = DefaultUserSettings.TabSize;

                return _tabSize;
            }
            set
            {
                if (value > 0)
                    _tabSize = value;
                else
                    _tabSize = DefaultUserSettings.TabSize;
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

                _selectedFileIndex = DefaultUserSettings.SelectedFileIndex;
                return _selectedFileIndex;
            }
            set
            {
                if (_selectedFileIndex >= 0 && _selectedFileIndex < Editors.Length)
                    _selectedFileIndex = value;
                else
                    _selectedFileIndex = DefaultUserSettings.SelectedFileIndex;
            }
        }

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

        public void RemoveInvalidFilePaths()
        {
            Editors = Editors.Where(x => File.Exists(x.FilePath)).ToArray();
        }

        /// <summary>
        ///     Adds distinct file paths to FilePaths
        /// </summary>
        /// <param name="paths"></param>
        public void AddFiles(params string[] paths)
        {
            // Add files to the end of the array
            AddFiles(Editors.Length, paths);
        }

        public void AddFiles(int index, params string[] paths)
        {
            var editors = Editors.ToList();
            
            // Adds distinct paths at specified index
            for (int i = 0; i < paths.Length; i++)
            {
                editors.Insert(i + index, new EditorInfo(HighlightingLanguage.None, paths[i]));
            }

            Editors = editors.ToArray();
        }

        public async void Save()
        {
            using (var streamWriter = new StreamWriter(SavePath, false))
            {
                var serializer = new XmlSerializer(typeof(UserSettings));
                serializer.Serialize(streamWriter, this);
            }
        }

        public static UserSettings Create()
        {
            var serializer = new XmlSerializer(typeof(UserSettings));

            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            try
            {
                using (var streamReader = new StreamReader(SavePath))
                {
                    var temp = (UserSettings) serializer.Deserialize(streamReader);
                    return temp;
                }
            }
            catch
            {
                using (var streamWriter = new StreamWriter(SavePath))
                {
                    serializer.Serialize(streamWriter, DefaultUserSettings);
                }
            }

            return DefaultUserSettings;
        }
    }
}