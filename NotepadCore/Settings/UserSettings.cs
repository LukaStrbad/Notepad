using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Newtonsoft.Json;
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
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\settings.json");

        private static readonly UserSettings DefaultUserSettings = new UserSettings
        {
            Editors = new EditorInfo[] { },
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
                    _editors = DefaultUserSettings.Editors;
                return _editors.Distinct(editor => editor.FilePath.ToLower()).ToArray();
            }
            set
            {
                _editors = value?.Distinct(editor => editor.FilePath.ToLower())
                        .ToArray() ?? DefaultUserSettings.Editors;
            }
        }

        public string EditorFontFamily
        {
            get => _editorFontFamily;
            set => _editorFontFamily = value;
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
                if (value >= 0 && value < Editors.Length)
                    _selectedFileIndex = value;
                else
                    _selectedFileIndex = 0;
            }
        }

        /// <summary>
        ///     Removes all occurances of the path
        /// </summary>
        /// <param name="path">Path to remove</param>
        private void RemoveFilePath(string path)
        {
            // Sets Editors if the path is different from the parameter
            Editors = Editors.Where(x => x.FilePath.ToLower() != path.ToLower()).ToArray();
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

        public void Save()
        {
            var serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });

            using var streamWriter = new StreamWriter(SavePath);
            using JsonWriter writer = new JsonTextWriter(streamWriter);
            serializer.Serialize(writer, this);
        }

        public static UserSettings Create()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });

            try
            {
                using var streamReader = new StreamReader(SavePath);
                using var reader = new JsonTextReader(streamReader);
                return serializer.Deserialize<UserSettings>(reader);
            }
            catch
            {
                using var streamWriter = new StreamWriter(SavePath);
                using var writer = new JsonTextWriter(streamWriter);
                serializer.Serialize(writer, DefaultUserSettings);
            }

            return DefaultUserSettings;
        }
    }
}