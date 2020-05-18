using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using NotepadCore.Exceptions;
using NotepadCore.Settings;
using NotepadCore.SyntaxHighlighters;

namespace NotepadCore
{
    
    
    
    public partial class MainWindow : Window
    {
        private int _newFileNumber = 1;

        public MainWindow()
        {
            // Inicijalizacija komponenti prozora
            InitializeComponent();

            // Stvaranje instance korisničkih postavki
            var userSettings = UserSettings.Create();

            // Ako postoje datoteke program ih učitava
            if (userSettings.Editors.Length != 0)
            {
                // Petlja koja prolazi kroz sve spremljene datoteke
                foreach (var i in userSettings.Editors)
                    try
                    {
                        // Dodavanje kartice za svaku datoteku
                        Tabs.Items.Insert(Tabs.Items.Count - 1, new TabItem
                        {
                            // Sadržaj kartice je TekstEditor kontrola
                            Content = new TextEditor(i.FilePath) { FileLanguage = i.HighlightingLanguage },
                            // Naslov kartice je ime datoteke
                            // Klasa FileInfo prima argument punu putanju datoteke te 
                            // sadrži svojstvo Name koje vraća kratko ime datoteke
                            Header = new FileInfo(i.FilePath).Name
                        });
                    }
                    catch
                    {
                    }
                // Odabir indeksa zadnje zapamćene otvorene kartice
                Tabs.SelectedIndex = userSettings.SelectedFileIndex;
            }
            // U slučaju da nema spremljenih datoteka, program dodaje praznu karticu
            else
            {
                // Umetanje prazne kartice na početak (indeks 0), svojstvo EmptyTab vraća 
                // praznu karticu
                Tabs.Items.Insert(0, EmptyTab);
                // Postavljanje odabranog indeksa na prvu karticu
                Tabs.SelectedIndex = 0;
            }

            // Mijenja font prema onaome koji je spremljen u postavkama
            ChangeFont();

            // Vraća prijašnje koordinate prozora (broj piksela od lijevo i od gore)
            Left = Properties.Settings.Default.LeftWindowPosition;
            Top = Properties.Settings.Default.TopWindowPosition;
            // Vraća prijašnju veličinu prozora (broj piksela širine i visine)
            Width = Properties.Settings.Default.WindowWidth;
            Height = Properties.Settings.Default.WindowHeight;
        }

        public TextEditor CurrentTextEditor => (TextEditor)Tabs.SelectedContent;

        
        
        
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Stvaranje instance korisničkih postavki
            var userSettings = UserSettings.Create();

            // Petlja koja prolazi kroz sve kartice isključujući zadnju koja služi samo za        
            // dodavanje novih kartica
            for (var i = 0; i < Tabs.Items.Count - 1; i++)
                try
                {
                    // i-ta kartica se pretvara u objekt TabItem čiji se sadržaj pretvara 
                    // kontrolu TextEditor
                    // Na kontroli TextEditor se poziva metoda SaveFile za spremanje 
                    // sadržaja
                    ((Tabs.Items[i] as TabItem).Content as TextEditor).SaveFile();
                }
                catch (InvalidSaveLocationException ex)
                {
                    // Ako se dogodi greška pri spremanju datoteke i ako postoji tekst u 
                    // kartici onda se postavlja upitnik za lokaciju spremanja
                    if (!string.IsNullOrEmpty(((Tabs.Items[i] as TabItem).Content as TextEditor).Text))
                    {
                        // Kartica sa nespremljenim sadržajem se postavlja u fokus
                        Tabs.SelectedIndex = i;
                        // Poziva se metoda koja je zadužena za spremanje datoteka
                        FileSave_Click(null, null);
                    }
                }

            // Sprema indeks zadnje korištene kartice
            userSettings.SelectedFileIndex = Tabs.SelectedIndex;
            // Uklanja nevažeće putanje pohrane
            userSettings.RemoveInvalidFilePaths();
            // Sprema korisničke postavke
            userSettings.Save();

            // Sprema koordinate prozora u pikselima
            Properties.Settings.Default.LeftWindowPosition = Left;
            Properties.Settings.Default.TopWindowPosition = Top;
            // Sprema širinu i visinu prozora u pikselima
            Properties.Settings.Default.WindowWidth = Width;
            Properties.Settings.Default.WindowHeight = Height;
            // Pozivanje metode koja pohranjuje spremljene vrijednosti na disk
            Properties.Settings.Default.Save();

            // Izlazak iz aplikacije
            Application.Current.Shutdown();
        }

        
        
        
        private void FileNew_Click(object sender, RoutedEventArgs e)
        {
            // Stvara instancu postavki
            var userSettings = UserSettings.Create();
            // Stvara novi dijaloški okvir za spremanje datoteka i prikazuje ga
            var newDialog = new SaveFileDialog();
            newDialog.ShowDialog();

            // Ako je putanja važeća (nije prazna ili null)
            if (!string.IsNullOrEmpty(newDialog.FileName))
            {
                // Dodaje novu datoteku u postavke
                userSettings.AddFiles(newDialog.FileName);
                // Umeće novu karticu sa putanjom do datoteke
                // Kartica se umeće na predzadnje mjesto zbog kartice +
                Tabs.Items.Insert(Tabs.Items.Count - 1, new TabItem
                {
                    // Sadržaj je novi TextEditor sa putanjom do datoteke
                    Content = new TextEditor(newDialog.FileName),
                    // Naslov kartice je naziv datoteke na putanji
                    Header = new FileInfo(newDialog.FileName).Name
                });
                // Sprema indeks kartice na predzadnji (prije kartice +) u postavke
                userSettings.SelectedFileIndex = userSettings.Editors.Length - 1;
                // Odabire predzadnju karticu
                Tabs.SelectedIndex = userSettings.Editors.Length - 1;
            }
            // Sprema postavke
            userSettings.Save();
        }

        
        
        
        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            // Stvara novu instancu postavki
            var userSettings = UserSettings.Create();
            // Stvara novu instancu okvira za otvaranje datoteka i otvara ga
            var openDialog = new OpenFileDialog();
            openDialog.ShowDialog();
            // U slučaju da je put do datoteke prazan izlazimo iz metode
            if (string.IsNullOrEmpty(openDialog.FileName))
                return;
            // Dodaje putanju do datoteke u postavke
            userSettings.AddFiles(openDialog.FileName);

            // Ukoliko je trenutna otvorena kartica prazna i nema putanju pohrane
            // otvara dokument u trenutnoj kartici
            if (string.IsNullOrWhiteSpace(CurrentTextEditor.Text) && !CurrentTextEditor.HasSaveLocation)
            {
                // Pretvaranje trenutno odabrane kartice u objekt TabItem
                var item = Tabs.SelectedItem as TabItem;
                // Mijenjamo sadržaj kartice u TextEditor sa putanjom datoteke
                item.Content = new TextEditor(openDialog.FileName);
                // Mijenjamo naslov kartice na naziv datoteke na putanji
                item.Header = new FileInfo(openDialog.FileName).Name;
            }
            // Inače otvaramo datoteku u predzadnjoj kartici jer je zadnja kartica +
            else
            {
                Tabs.Items.Insert(Tabs.Items.Count - 1, new TabItem
                {
                    // Mijenjamo sadržaj kartice u TextEditor
                    Content = new TextEditor(openDialog.FileName),
                    // Mijenjamo naslov kartice u ime datoteke
                    Header = new FileInfo(openDialog.FileName).Name
                });
                // Odabiremo predzadnju karticu
                Tabs.SelectedIndex = Tabs.Items.Count - 2;
            }
            // Spremanje korisničkih postavki
            userSettings.Save();
        }

        private void FileClose_Click(object sender, RoutedEventArgs e)
        {
            // Kreira instancu korisničkih postavki
            var userSettings = UserSettings.Create();
            // Ako postoje dvije kartice (datoteka i + kartica) i
            // ako datoteka nema lokaciju za pohranu a ima teksta pitaj korisnika za    
            // lokaciju spremanja
            if (!string.IsNullOrWhiteSpace(CurrentTextEditor.Text) && !CurrentTextEditor.HasSaveLocation &&
                Tabs.Items.Count == 2)
            {
                FileSave_Click(sender, e);
                // Uklanja trenutnu datoteku iz postavki i sprema postavke
                userSettings.RemoveFilePaths(CurrentTextEditor.DocumentPath);
                userSettings.Save();
                // Mijenja trenutnu karticu sa praznom karticom
                Tabs.Items[Tabs.SelectedIndex] = EmptyTab;
                Tabs.SelectedIndex = 0;
                return;
            }
            // Ako su dvije kartice i ako je datoteka prazna i nema lokaciju spremanja ne 
            // trebamo raditi ništa, program izlazi iz metode
            if (string.IsNullOrWhiteSpace(CurrentTextEditor.Text) && !CurrentTextEditor.HasSaveLocation &&
                Tabs.Items.Count == 2)
                return;

            // Pretvaramo polje u listu za lakše uklanjanje datoteka 
            var files = userSettings.Editors.ToList();
            // Spremamo datoteku ako ima lokaciju za spremanje
            if (CurrentTextEditor.HasSaveLocation)
                CurrentTextEditor.SaveFile();
            // Ako nema lokaciju za spremanje a ima teksta pitaj korisnika da je spremi 
            else if (!string.IsNullOrWhiteSpace(CurrentTextEditor.Text))
                FileSave_Click(sender, e);
            // Ako je odabrana prva karticu zatvaramo prvu karticu sa indeksom
            if (Tabs.SelectedIndex == 0)
            {
                Tabs.Items.RemoveAt(0);
                files.RemoveAt(0);
            }
            // Inače možemo samo dekrementirati jer neće doći do greške
            else
            {
                Tabs.SelectedIndex--; // Postavimo prijašnju karticu u fokus

                // Uklonimo traženu karticu iz kartica i iz spremljenih editora
                Tabs.Items.RemoveAt(Tabs.SelectedIndex + 1);
                files.RemoveAt(Tabs.SelectedIndex + 1);
            }

            // Spremanje izmijenjene kolekcije editora u postavke
            userSettings.Editors = files.ToArray();
            // Spremanje korisničkih postavki
            userSettings.Save();
        }

        
        
        
        private void FileSave_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = UserSettings.Create(); // Stvara instancu postavki
                                                      // Ako trenutni dokument ima lokaciju spremanja, spremamo dokument
            if (CurrentTextEditor.HasSaveLocation)
            {
                CurrentTextEditor.SaveFile();
            }
            // Inače pitamo korisnika gdje želi da spremimo datoteku
            else
            {
                // Kreira instancu dijaloga za spremanje datoteke i prikazuje ga
                var saveDialog = new SaveFileDialog();
                saveDialog.ShowDialog();
                // Ukoliko je putanja prazna izlazimo iz metode
                if (string.IsNullOrEmpty(saveDialog.FileName))
                    return;
                // Sprema putanju datoteke u editor
                CurrentTextEditor.DocumentPath = saveDialog.FileName;
                var paths = userSettings.Editors.ToList();
                // Ubacujemo novi editor u postavke
                paths.Insert(Tabs.SelectedIndex,
                    new EditorInfo(HighlightingLanguage.None, CurrentTextEditor.DocumentPath));
                // Spremanje liste editora
                userSettings.Editors = paths.ToArray();
                ((TabItem)Tabs.Items[Tabs.SelectedIndex]).Header = CurrentTextEditor.FileName;
                try
                {
                    // Pokušaj spremanja datoteke
                    CurrentTextEditor.SaveFile();
                }
                catch
                {
                }
                // Spremanje korisničkih postavki
                userSettings.Save();
            }
        }

        
        
        
        private void FileSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = UserSettings.Create(); // Stvara instancu postavki

            // Ako postoji lokacija za spremanje sprema sadržaj trenutne datoteke
            if (CurrentTextEditor.HasSaveLocation)
                CurrentTextEditor.SaveFile();
            // Stvara instancu dijaloga za spremanje datoteke i prikazuje ga
            var saveDialog = new SaveFileDialog();
            saveDialog.ShowDialog();

            // Ukoliko je putanja prazna izlazimo iz metode 
            if (string.IsNullOrEmpty(saveDialog.FileName))
                return;

            // Brišemo putanju trenutne datoteke iz postavki
            userSettings.RemoveFilePaths(CurrentTextEditor.DocumentPath);

            // Korištenjem objekta StreamWriter spremamo sadržaj datoteke u odabranu 
            // datoteku
            // Drugi argument konstruktora je false tako da se obriše sadržaj datoteke ako 
            // posotji na odabranoj putanji
            using (var sw = new StreamWriter(saveDialog.FileName, false))
            {
                // U datoteku se sprema sadržaj trenutnog editora
                sw.Write(CurrentTextEditor.Text);
            }

            // Dodjeljujemo novu putanju trenutnom editoru
            CurrentTextEditor.DocumentPath = saveDialog.FileName;
            // Spremanje sadržaja trenutnog editora
            CurrentTextEditor.SaveFile();
            // Dodajemo novu putanju u postavke
            userSettings.AddFiles(Tabs.SelectedIndex, saveDialog.FileName);

            userSettings.Save();
            // Mijenjamo naslov kartice prema nazivu datoteke
            ((TabItem)Tabs.SelectedItem).Header = CurrentTextEditor.FileName;
        }


        
        
        
        public void ChangeFont()
        {
            var userSettings = UserSettings.Create(); // Stvara instancu postavki

            // Prolazimo kroz sve otvorene editore
            foreach (var textEdit in GetTextEditors())
            {
                // Pozivanje metode ChangeFont na svakom editoru
                textEdit.ChangeFont();
            }
        }

        
        
        
        private void FindReplace_Click(object sender, RoutedEventArgs e)
        {
            // Izlazi iz metode ako postoji otvoreni prozor Find
            if (Application.Current.Windows.OfType<Find>().Any())
                return;

            // Prikazuje prozor Find
            new Find().Show();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // Prikazuje dijaloški okvir u obliku dijaloga
            // Kada se otvori prozor u obliku dijaloga, on onemogućuje interakciju sa    
            // ostalim prozorima u aplikaciji
            // Svojstvo Owner označava da je vlasnik trenutni prozor
            new SettingsWindow { Owner = this }.ShowDialog();
        }

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ako je odabrana kartica za dodavanje (+)
            if (ReferenceEquals(Tabs.SelectedItem, TabAdd))
            {
                // Stvaranje instance korisničkih postavki
                var userSettings = UserSettings.Create();
                // Dodajemo novu prazni karticu na kraj
                Tabs.Items.Insert(Tabs.Items.Count - 1, EmptyTab);
                // Dodajemo praznu putanju kako bi u postavkama imali jednak broj editora
                userSettings.AddFiles("");
                userSettings.Save();
                // Dekrementiramo indeks odabrane kartice (biramo onu karticu prije +)
                Tabs.SelectedIndex--;
            }
        }

        private TabItem EmptyTab => new TabItem
        {
            // Sadržaj prazne kartice je novi TextEditor
            Content = new TextEditor { DocumentPath = "" },
            // Naslov kartice počinje sa 1 i uvećava se svakim čitanjem svojstva
            Header = $"new file {_newFileNumber++}"
        };

        public IEnumerable<TextEditor> GetTextEditors()
        {
            // Prolazimo kroz sve objekte kartica
            foreach (object item in Tabs.Items)
            {
                // Ako je sadržaj kartice tipa TextEditor vraćamo ga
                if ((item as TabItem)?.Content is TextEditor textEditor)
                    yield return textEditor;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            // Fokusiranje kontrole Tabs
            Tabs.Focus();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            // Fokusiranje glavnog editora trenutno odabrane kartice
            CurrentTextEditor.MainTextBox.Focus();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            // Poruka o autoru i kreatoru ikone aplikacije
            string message = "Application made by Luka Strbad\n" +
                        "Icon made by https://www.flaticon.com/authors/smashicons from www.flaticon.com";
            // Pozivanje metode Show klase MessageBox za prikaz prozora sa porukom
            MessageBox.Show(message, "About", MessageBoxButton.OK, MessageBoxImage.None);
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            // Ako je pritisnuta kombinacija tipki Ctrl + T odabiremo karticu za dodavanje
            // kako bi dodali novu karticu
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.T)
            {
                Tabs.SelectedIndex = Tabs.Items.Count - 1;
                // Označavamo da je event gotov tako da se ne bi neki drugi izvršio
                e.Handled = true;
            }
            // Ako je pritisnuta kombinacija tipki Ctrl + tab
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.Tab)
            {
                // Ako je uz te dvije tipke pritisnuta i tipka shift program prolazi kroz
                // kartice unazad
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    // Ako je odabrana prva kartice, program odabire
                    if (Tabs.SelectedIndex == 0)
                        Tabs.SelectedIndex = Tabs.Items.Count - 2;
                    // Inače program smanji indeks odabrane kartice za 1
                    else
                        Tabs.SelectedIndex--;
                }
                // Ako su pritisnute samo te dvije tipke program prolazi kroz kartice 
                // unaprijed
                else
                {
                    // Ako je odabrana zadnja kartica, program odabire prvu
                    if (Tabs.SelectedIndex == Tabs.Items.Count - 2)
                        Tabs.SelectedIndex = 0;
                    // Inače program poveća indeks kartice za 1
                    else
                        Tabs.SelectedIndex++;
                }
                // Označavamo da je event gotov tako da se ne bi neki drugi izvršio
                e.Handled = true;
            }
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            // Postavljanje sadržaja međuspremnika
            Clipboard.SetDataObject(CurrentTextEditor.MainTextBox.Selection.Text);
            // Brisanje trenutno odabranog teksta
            CurrentTextEditor.MainTextBox.Selection.Text = "";
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            // Postavljanje sadržaja međuspremnika
            Clipboard.SetDataObject(CurrentTextEditor.MainTextBox.Selection.Text);
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            // Na mjesto kursora, program lijepi tekst
            CurrentTextEditor.MainTextBox.CaretPosition.InsertTextInRun(Clipboard.GetText());
        }

    }
}