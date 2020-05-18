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
    public sealed class UserSettings
    {
        // Svojstvo je samo za čitanje
        // Putanja je kombinacija putanje baznog direktorija i datoteke settings.xml
        // koja se nalazi u direktoriju Data
        private static readonly string SavePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\settings.xml");

        private static readonly UserSettings DefaultUserSettings = new UserSettings
        {
            // Editori su prazni
            Editors = new EditorInfo[] {new EditorInfo(),},
            // Zadan font je Consolas
            EditorFontFamily = "Consolas",
            // Zadana veličina fonta je 12
            EditorFontSize = 12,
            // Zadani broj razmake pri pritisku tipke tab je 4
            TabSize = 4,
            // Zadani indeks odabrane kartice je 0
            SelectedFileIndex = 0
        };

        // Varijabla za spremanje obitelji fonta
        private string _editorFontFamily;

        // Varijabla za spremanje veličine fonta
        private int _editorFontSize;

        // Varijabla za spremanje informacija o editorima
        private EditorInfo[] _editors;

        // Varijabla za spremanje indeksa odabrane kartice
        private int _selectedFileIndex;

        // Varijabla za spremanje broja razmaka
        private int _tabSize;

        private UserSettings()
        {
        }

        public EditorInfo[] Editors
        {
            get
            {
                // Ako je varijabla _editors prazna postavljamo je po zadanoj postavki
                if (_editors == null)
                    _editors = new EditorInfo[] { };
                // Vraćanje editora koji imaju različite putanje
                return _editors.Distinct(editor => editor.FilePath.ToLower()).ToArray();
            }
            set
            {
                // Ako vrijednost nije null, u varijablu _editors se spremaju samo editori
                // sa različitim putanjama, a ako je vrijednost null, spremaju se zadane
                // postavke
                _editors = value?.Distinct(editor => editor.FilePath.ToLower()).ToArray() ??
                           new[] {new EditorInfo()};
            }
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
                // Ako je vrijednost manja od 8 ili veća od 96 postavlja se zadana 
                // vrijednost
                if (!(_editorFontSize >= 8 && _editorFontSize <= 96))
                    _editorFontSize = DefaultUserSettings.EditorFontSize;
                // Vraćanje vrijednosti varijable
                return _editorFontSize;
            }
            set
            {
                // Ako je vrijednost veća ili jednaka od 8 i manja ili jednaka 96 
                // postavlja se vrijednost u varijablu
                if (value >= 8 && value <= 96)
                    _editorFontSize = value;
                // Ako nije postavlja se zadana vrijednost
                else
                    _editorFontSize = DefaultUserSettings.EditorFontSize;
            }
        }

        public int TabSize
        {
            get
            {
                // Ako je veličina manja ili jedna 0 postavlja se zadana vrijednost
                if (_tabSize <= 0)
                    _tabSize = DefaultUserSettings.TabSize;
                // Vraćanje vrijednosti
                return _tabSize;
            }
            set
            {
                // Ako je vrijednost veća od nula sprema se u varijablu
                if (value > 0)
                    _tabSize = value;
                // Ako nije spremaju se zadane postavke
                else
                    _tabSize = DefaultUserSettings.TabSize;
            }
        }

        public int SelectedFileIndex
        {
            get
            {
                // Ako je indeks veći ili jednak nula i manji od broja editora program
                // vraća indeks
                if (_selectedFileIndex >= 0 && _selectedFileIndex < Editors.Length)
                    return _selectedFileIndex;
                // Ako indeks nije u dozvoljenom rasponu postavlja se zadana vrijednost
                _selectedFileIndex = DefaultUserSettings.SelectedFileIndex;
                return _selectedFileIndex;
            }
            set
            {
                // Ako je indeks veći ili jednak nula i manji od broja editora program
                // postavlja vrijednost
                if (value >= 0 && value < Editors.Length)
                    _selectedFileIndex = value;
                // Ako indeks nije u dozvoljenom rasponu postavlja se zadana vrijednost
                else
                    _selectedFileIndex = 0;
            }
        }

        private void RemoveFilePath(string path)
        {
            // Postavljanje polja Editors tako da sadrži samo one putanje koje su 
            // različite od argumenta metode
            Editors = Editors.Where(x => x.FilePath.ToLower() != path.ToLower()).ToArray();
        }

        public void RemoveFilePaths(params string[] paths)
        {
            // Program prolazi kroz svaku putanju i poziva metodu RemoveFilePath
            foreach (var path in paths)
                RemoveFilePath(path);
        }

        public void RemoveInvalidFilePaths()
        {
            // Postavljanje polja Editors na polje gdje je izvršena provjera postoje li 
            // datoteke na putanjama
            Editors = Editors.Where(x => File.Exists(x.FilePath)).ToArray();
        }


        public void AddFiles(params string[] paths)
        {
            // Pozivanje metode AddFiles sa početnim indeksom koji je jednak veličini
            // polja Editors
            AddFiles(Editors.Length, paths);
        }

        public void AddFiles(int index, params string[] paths)
        {
            // Pretvaranje polja Editors u listu radi lakšeg umetanja elemenata
            var editors = Editors.ToList();

            // Polje koje kreće od 0 i ide o veličine polja argumenta
            for (int i = 0; i < paths.Length; i++)
            {
                // U listu editors program umeće putanje na određeni indeks
                editors.Insert(i + index, new EditorInfo(HighlightingLanguage.None, paths[i]));
            }
            // Spremanje promijenjene vrijednosti u polje Editors
            Editors = editors.ToArray();
        }

        public void Save()
        {
            // Stvaranje novog StreamWriter objekta sa lokacijom pohrane SavePath
            using (var streamWriter = new StreamWriter(SavePath, false))
            {
                // Stvaranje novog XmlSerializer objekta sa argumentom koji je tip klase
                // UserSettings
                var serializer = new XmlSerializer(typeof(UserSettings));
                // Metoda Serialize sprema svojstva vrijednosti trenutnog objekta u 
                // datoteku objekta streamWriter
                serializer.Serialize(streamWriter, this);
            }
        }

        public static UserSettings Create()
        {
            // Stvaranje novog XmlSerializer objekta sa argumentom koji je tip klase
            // UserSettings
            var serializer = new XmlSerializer(typeof(UserSettings));

            // Putanja do direktorija Data koji se nalazi u baznom direktoriju aplikacije
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            // Ako direktorij ne postoji, program ga stvara
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // Pokušaj čitanja postavki
            try
            {
                // Stvaranje novog StreamReader objekta sa putanjom SavePath
                using (var streamReader = new StreamReader(SavePath))
                {
                    // Metoda Deserialize učitava postavke iz datoteke i vraća ih u obliku
                    // objekta UserSettings
                    var temp = (UserSettings) serializer.Deserialize(streamReader);
                    // Vraćanje novog objekta
                    return temp;
                }
            }
            // Ako nije moguće pročitati postavke zapisuju se zadane postavke
            catch
            {
                // Stvaranje novog StreamReader objekta sa putanjom SavePath
                using (var streamWriter = new StreamWriter(SavePath))
                {
                    // Spremanje svojstava zadanih postavki
                    serializer.Serialize(streamWriter, DefaultUserSettings);
                }
            }
            // Vraćanje zadanih postavki u slučaju da nije bilo moguće pročitati postavke
            return DefaultUserSettings;
        }
    }
}