using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using System.Xml;

namespace Notepad
{
    /// <summary>
    /// A class that stores user settings
    /// </summary>
    public class Settings
    {
        private Settings() { }

        private static readonly string SavePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\settings.xml");

        private static Settings DefaultUserSettings = new Settings()
        {
            FilePaths = new string[] { "" },
            EditorFontFamily = "Consolas",
            EditorFontSize = 12,
            TabSize = 4,
            UseTabs = false,
            SelectedFileIndex = 0,
            ShowLineNumbers = true,
            UseSpaces = true
        };

        private string[] _filePaths;
        private string _editorFontFamily;
        private int _editorFontSize;
        private int _tabSize;
        private int _selectedFileIndex;

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
        /// Gets or sets a custom tab size
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
        /// Gets saved file paths
        /// </summary>
        public string[] FilePaths
        {
            get
            {
                return _filePaths.Select(x => x.ToLower()).Distinct().ToArray() ?? DefaultUserSettings.FilePaths;
            }
            set => _filePaths = value.Select(x => x.ToLower()).Distinct().ToArray() ?? DefaultUserSettings.FilePaths;
        }

        /// <summary>
        /// Removes all occurances of the path
        /// </summary>
        /// <param name="path">Path to remove</param>
        private void RemoveFilePath(string path)
        {
            // if there are multiple occurances
            while (FilePaths.Contains(path))
                FilePaths = FilePaths.Where(x => x != path).ToArray();
        }

        /// <summary>
        /// Removes all occurances of paths in the array
        /// </summary>
        /// <param name="paths">Paths to remove</param>
        public void RemoveFilePaths(params string[] paths)
        {
            foreach (var path in paths)
                RemoveFilePath(path);
        }

        /// <summary>
        /// Adds distinct file paths to FilePaths
        /// </summary>
        /// <param name="paths"></param>
        public void AddFiles(params string[] paths)
        {
            // adds distincs paths to FilePaths setting
            FilePaths = new string[][] { FilePaths, paths.ToArray() }.SelectMany(x => x).Distinct().ToArray();
        }

        public int SelectedFileIndex
        {
            get
            {
                if (_selectedFileIndex >= 0 && _selectedFileIndex < FilePaths.Length)
                    return _selectedFileIndex;

                _selectedFileIndex = DefaultUserSettings.SelectedFileIndex;
                return _selectedFileIndex;
            }
            set
            {
                if (_selectedFileIndex >= 0 && _selectedFileIndex < FilePaths.Length)
                    _selectedFileIndex = value;
                else
                    _selectedFileIndex = DefaultUserSettings.SelectedFileIndex;
            }
        }

        public bool UseTabs { get; set; }

        public bool ShowLineNumbers { get; set; } = true;

        public bool UseSpaces { get; set; } = true;

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

            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            try
            {
                using (var streamReader = new StreamReader(SavePath))
                {
                    var temp = (Settings)serializer.Deserialize(streamReader);
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