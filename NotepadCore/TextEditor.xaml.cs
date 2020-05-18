using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using NotepadCore.Annotations;
using NotepadCore.Exceptions;
using NotepadCore.ExtensionMethods;
using NotepadCore.SyntaxHighlighters;

namespace NotepadCore
{
    
    
    
    public partial class TextEditor : UserControl, INotifyPropertyChanged
    {
        // Putanja pohrane
        private string _documentPath;
        // Prijašnji broj linija, pomaže u brzini rada
        private int _oldNumberOfLines = -1;
        // Jezik isticanja datoteke
        private HighlightingLanguage _fileLanguage;
        // Veličina tab-a
        private int _tabSize;
        // Lista koja sprema lokacije svih komentara za isticanje
        private List<TextRange> _comments = new List<TextRange>();

        public HighlightingLanguage FileLanguage
        {
            // Vraća svojstvo _fileLanguage
            get => _fileLanguage;
            set
            {
                // Sprema vrijednost i poziva event 
                _fileLanguage = value;
                OnPropertyChanged();

                // Stvara instancu korisničkih postavki
                var userSettings = Settings.UserSettings.Create();
                // Petlja koja prolazi kroz sve spremljene editore da bi se pronašao 
                // trenutni zato što trenutni TextEditor ne zna svoje indeks
                foreach (var editor in userSettings.Editors)
                {
                    // Ako se podudaraju putanje spremanja i nisu prazni sprema vrijednost
                    if (editor.FilePath?.ToLower() == DocumentPath?.ToLower() && DocumentPath != null
                    ) // Find current editor
                    {
                        editor.HighlightingLanguage = value;
                        break;
                    }
                }
                // Spremanje postavki
                userSettings.Save();
            }
        }

        public int TabSize
        {
            // Vraća vrijednost varijable _tabSize
            get => _tabSize;
            set
            {
                // Postavlja vrijednost ako je veća od 0
                if (value > 0)
                    _tabSize = value;
                // Ako nije veća od 0 baca grešku
                else throw new ArgumentException("Tab size can't be less than zero or zero");
            }
        }

        public string DocumentPath
        {
            // Vraća svojstvo _documentPath, putanja datoteke
            get => _documentPath;
            set
            {
                // Ako datoteka postoji, program učitava sadržaj i postavlja ga u 
                // TextEditor
                if (File.Exists(value))
                    Text = File.ReadAllText(value);
                // Inače tekst je prazan
                else
                    Text = "";
                // Sprema putanju do datoteke
                _documentPath = value;
            }
        }

        private IHighlighter Highlighter =>
            FileLanguage switch
            {
                // Ako je jezik C# ili Markup vraća određeni Highlighter
                HighlightingLanguage.CSharp => new CSharpHighlighter(),
                HighlightingLanguage.MarkupLanguage => new MarkupHighlighter(),
                HighlightingLanguage.JSON => new JSONHighlighter(),
                // Ako nijedan od slučajeva nije zadovoljen, vraća prazan Highlighter
                _ => new EmptyHighlighter()
            };

        public TextEditor()
        {
            InitializeComponent();
            // Postavljamo kontekst podataka za LanguageComboBox na trenutni objekt
            LanguageComboBox.DataContext = this;
            // Stvaranje instance korisničkih postavki
            var userSettings = Settings.UserSettings.Create();
            // Fokusira glavni textbox
            MainTextBox.Focus();
            // Mijenja font
            ChangeFont();
            // Mijenja veličinu tab-a ovisno o postavkama
            TabSize = userSettings.TabSize;
            // Određuje hoće li se prikazati LineTextBox
            ShowLineNumbers = Properties.Settings.Default.ShowLineNumbers;
            // Nema zadanog jezika za isticanje
            FileLanguage = HighlightingLanguage.None;
            // Pozivanje metode MainTextBox_OnSelectionChanged kako bi program napisao 
            // informacije o mjestu kursora u tekstu
            MainTextBox_OnSelectionChanged(this, null);
        }

        public TextEditor(string documentPath) : this() // Poziv na prijašnji konstruktor
        {
            // Sprema putanju pohrane i učitava tekst ako datoteka postoji
            DocumentPath = documentPath;

            if (!HasSaveLocation)
            {
                // Ako datoteka ne postoji pokušava stvoriti novu datoteku
                try
                {
                    File.Create(DocumentPath);
                }
                catch
                {
                }
            }
        }

        // Returns true if file exists
        public bool HasSaveLocation => File.Exists(DocumentPath);

        public string FileName
        {
            get
            {
                // Ako postoji putanja do datoteke vraća ime
                if (HasSaveLocation)
                    return new FileInfo(DocumentPath).Name;
                // Ako ne vraća prazan string
                return "";
            }
        }


        public bool ShowLineNumbers
        {
            // Ako je LineTextBox vidljiv vraća true, inače vraća false
            get => LineTextBox.Visibility == Visibility.Visible;
            set
            {
                // Ako je vrijednost true postavljamo LineTextBox da bude vidljiv
                if (value)
                {
                    // Vidljivost je Visible
                    LineTextBox.Visibility = Visibility.Visible;
                    // Za glavni tekstualni editor:
                    // - Postavljamo redak u 1 (2. redak)
                    MainTextBox.SetValue(Grid.ColumnProperty, 1);
                    // Postavljamo raspon stupca na 1 tako da zauzima 1 redak
                    MainTextBox.SetValue(Grid.ColumnSpanProperty, 1);
                }
                // Ako je vrijednost false
                else
                {
                    // Vidljivost je Collapsed, odnosno nevidljivo
                    LineTextBox.Visibility = Visibility.Collapsed;
                    // Za glavni tekstualni editor:
                    // - Postavljamo redak u 0 (1. redak)
                    MainTextBox.SetValue(Grid.ColumnProperty, 0);
                    // Postavljamo raspon stupca na 2 tako da zauzme oba retka
                    MainTextBox.SetValue(Grid.ColumnSpanProperty, 2);
                }
            }
        }

        public string Text
        {
            get
            {
                //Stvaramo novi objekt tipa TextRange koji se prostire po cijeloj datoteci
                var textRange = new TextRange(MainTextBox.Document.ContentStart, MainTextBox.Document.ContentEnd);
                // Svojstvo Text vraća cjelokupan tekst objekta textRange
                return textRange.Text;
            }
            set
            {
                // Briše sve blokove dokumenta
                MainTextBox.Document.Blocks.Clear();
                // Vrijednost teksta se trim-a
                var paragraphs = value.Trim()
                    // Tekst se razdvoji tamo gdje su bile nove linije
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                    // Razdvojene linije se pretvore u IEnumerable<Paragraph>
                    .Select(a => new Paragraph(new Run(a)));
                MainTextBox.Document.Blocks.AddRange(paragraphs);

                // Privremeno uklanjanje metode iz eventa TextChanged
                MainTextBox.TextChanged -= MainTextBox_TextChanged;
                // Poziv metode za isticanje teksta
                HighlightRange();
                // Poziva metode za isticanje komentara
                HighlightCommentsInRange();
                // Vraćanje metode u event TextChanged
                MainTextBox.TextChanged += MainTextBox_TextChanged;

            }
        }

        
        
        
        public void ChangeFont()
        {
            // Stvaranje instance korisničkih postavki
            var userSettings = Settings.UserSettings.Create();

            // Stvaranje instance fonta
            var fontFamily = new FontFamily(userSettings.EditorFontFamily);
            // Mijenja obitelj fonta za glavni textbox
            MainTextBox.FontFamily = fontFamily;
            // Mijenja veličinu fonta za glavni textbox
            MainTextBox.FontSize = userSettings.EditorFontSize;
            // Isto i za textbox koji prikazuje linije
            LineTextBox.FontFamily = fontFamily;
            LineTextBox.FontSize = userSettings.EditorFontSize;
        }

        
        
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Ako postoji putanja pohrane
            if (HasSaveLocation)
            {
                // Stvara novi objekt tipa FileInfo za informacije o datoteci
                var fileInfo = new FileInfo(DocumentPath);

                // Ako nastavak datoteke završava na "ml" jezik je MarkupLanguage
                if (fileInfo.Extension.EndsWith("ml"))
                    FileLanguage = HighlightingLanguage.MarkupLanguage;
                // Ako nastavak datoteke ne završava na "ml"
                else
                    FileLanguage = fileInfo.Extension switch
                    {
                        // Jezik je C# ako datoteka završava na ".cs"
                        ".cs" => HighlightingLanguage.CSharp,
                        // Jezik je JSON ako datoteka završava na ".json"
                        ".json" => HighlightingLanguage.JSON,
                        // Inače nema jezika isticanja
                        _ => HighlightingLanguage.None
                    };
            }
            // Ispisuje brojeve linija
            WriteLineNumbers();
        }

        
        
        
        private void MainTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Broj linija je jednak broj blokova u dokumentu
            var lineCount = MainTextBox.Document.Blocks.Count;
            // Ako je novi broj linija različit od prethodnog puta
            if (_oldNumberOfLines != lineCount)
            {
                // Ispisuje brojeve linija
                WriteLineNumbers();
                // Ažurira stari broj linija
                _oldNumberOfLines = lineCount;
            }
            // Privremeno uklanjanje metode iz eventa TextChanged
            MainTextBox.TextChanged -= MainTextBox_TextChanged;
            // Isticanje trenutne linije
            HighlightCurrentLine();
            // Poziva metode za isticanje komentara
            HighlightCommentsInRange();
            // Vraćanje metode u event TextChanged
            MainTextBox.TextChanged += MainTextBox_TextChanged;
        }

        private void HighlightCurrentLine()
        {
            // Ako nema jezika isticanja program izlazi iz metode
            if (FileLanguage == HighlightingLanguage.None)
                return;
            // Pozivanje metode HighlightRange koja kao argument šalje vrijednosti početka 
            // i kraja trenutne linije
            HighlightRange(MainTextBox.CaretPosition.Paragraph.ContentStart,
                MainTextBox.CaretPosition.Paragraph.ContentEnd);
        }

        private void HighlightRange(TextPointer start = null, TextPointer end = null)
        {
            // Ako glavni textbox nije incijaliziran program izlazi iz metode
            if (MainTextBox == null) return;
            // Stvaranje novog objekta tipa TextRange koji se proteže od početka do kraja  
            // zadanih u parametrima funkcije
            // Ako je parametar null, vrijednost će biti početak odnosno kraj dokumenta
            var textRange = new TextRange(start ?? MainTextBox.Document.ContentStart,
                end ?? MainTextBox.Document.ContentEnd);

            // Brišemo sva svojstva textRange objekta da bi program uklonio boju iz teksta
            textRange.ClearAllProperties();

            try
            {
                // Postavljanje novih varijabli offset i prevIndex na početne vrijednosti
                TextPointer offset = null;
                int prevIndex = -1;
                // Petlja koja prolazi kroz svaki pogodak za trenutni jezik isticanja
                // pogoci su sortirani po indeksu
                foreach (var (match, brush) in Highlighter.GetMatches(textRange).OrderBy(x => x.Match.Index))
                {
                    // Ako je offest na početnoj vrijednosti
                    if (offset == null)
                    {
                        // Postavljanje varijable offset na indeks od prvog pogotka
                        offset = textRange.Start.GetTextPointerAtOffset(match.Index);
                        // Postavljanje varijable prevIndex na prvi indeks
                        prevIndex = match.Index;
                    }
                    // Offset se povećava za razliku od trenutnog indeksa i prošlog
                    // što znači da se mora pomaknuti od prošlog na trenutno mjesto
                    offset = offset.GetTextPointerAtOffset(match.Index - prevIndex);
                    // Stvaranje privremenog TextRange objekta koji se proteže od 
                    // mjesta na varijabli offset do mjesta koje je udaljeno od te 
                    // varijable za dužinu pogotka
                    // Privremeni TextRange primjenjuje svojstvo boje 
                    new TextRange(offset, offset.GetTextPointerAtOffset(match.Length))
                        .ApplyPropertyValue(TextElement.ForegroundProperty, brush);
                    // Ažuriranje varijable prevIndex sa trenutnim indeksom
                    prevIndex = match.Index;
                }
            }
            catch
            {
            }
        }

        private bool UndoCommentsInRange(bool highlightLater = true)
        {
            // Ako je glavni TextBox null, ili lista _comments null ili ne postoje  
            // spremljeni komentari program izlazi iz metode i vraća vrijednost false
            if (MainTextBox == null || _comments == null || !_comments.Any())
                return false;

            try
            {
                // Petlja koja prolazi kroz sve spremljene komentare
                foreach (var comment in _comments)
                {
                    // Brisanje boje komentara
                    comment.ClearAllProperties();
                    // Ako je svojstvo highlightLater true, izbrisana boja se ponovno 
                    // označuje kao normalan tekst
                    if (highlightLater)
                        HighlightRange(comment.Start, comment.End);
                }
            }
            catch (Exception e)
            {
            }
            // Brijanje liste komentara
            _comments.Clear();
            return true;
        }

        private void HighlightCommentsInRange(TextPointer start = null, TextPointer end = null,
     bool saveComments = true, bool highlightLater = true)
        {
            // Ako je glavni TextBox null program izlazi iz metode
            if (MainTextBox == null)
                return;

            // Stvaranje novog TextRange objekta koji se proteže između početka i kraja 
            // argumenata metode ili glavnog TextBox-a
            var textRange = new TextRange(start ?? MainTextBox.Document.ContentStart,
                end ?? MainTextBox.Document.ContentEnd);
            // Ako nisu uklonjeni komentari program označuje tekst
            if (!UndoCommentsInRange(highlightLater))
                HighlightRange(start, end);

            try
            {
                // Postavljanje novih varijabli offset i prevIndex na početne vrijednosti
                TextPointer offset = null;
                int prevIndex = -1;
                // Petlja koja prolazi kroz svaki pogodak za trenutni jezik isticanja
                // pogoci su sortirani po indeksu
                foreach (var (match, brush) in Highlighter.GetCommentMatches(textRange).OrderBy(x => x.Match.Index))
                {
                    // Ako je offset na početnoj vrijednosti
                    if (offset == null)
                    {
                        // Postavljanje varijable offset na indeks od prvog pogotka
                        offset = textRange.Start.GetTextPointerAtOffset(match.Index);
                        // Postavljanje varijable prevIndeks na prvi indeks
                        prevIndex = match.Index;
                    }
                    // Offset se povećava za razliku od trenutnog indeksa i prošlog
                    offset = offset.GetTextPointerAtOffset(match.Index - prevIndex);
                    // Stvaranje privremenog TextRange objekta koji obuhvaća trenutni 
                    // pogodak
                    var tempRange = new TextRange(offset, offset.GetTextPointerAtOffset(match.Length));
                    // Primjenjivanje boje na TextRange
                    tempRange.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
                    // Ako se komentari spremanju, TextRange se sprema u listu _comments
                    if (saveComments)
                        _comments.Add(tempRange);
                    // Ažuriranje varijable na trenutni indeks
                    prevIndex = match.Index;
                }
            }
            catch
            {
            }
        }


        private void MainTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Broj linija jednak je broju blokova u dokumentu
            var lineCount = MainTextBox.Document.Blocks.Count;
            // Ako je broj linija jednak kao i prije program izlazi iz metode
            if (_oldNumberOfLines == lineCount) return;
            // Ako se metoda nastavlja ispisuje se broj linija i ažurira broj linija
            WriteLineNumbers();
            _oldNumberOfLines = lineCount;
        }

        
        
        
        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Ako je pozivatelj metode glavni textbox, pomak textbox-a za linije se 
            // sinkronizira sa glavnim
            if (ReferenceEquals(sender, MainTextBox))
                LineTextBox.ScrollToVerticalOffset(MainTextBox.VerticalOffset);
            // Ako je pozivatelj metode textbox za linije, pomak glavnog textbox-a se 
            // sinkronizira sa linijskim 
            else if (ReferenceEquals(sender, LineTextBox))
                MainTextBox.ScrollToVerticalOffset(LineTextBox.VerticalOffset);
        }


        
        
        
        private void WriteLineNumbers()
        {
            // Stvaranje objekta tipa StringBuilder zbog efikasnosti
            var sb = new StringBuilder();
            // U petlji prolazimo od 1 do trenutnog broja linija i u StringBuilder 
            // dodajemo broj u obliku tipa string
            for (var i = 1; i <= MainTextBox.Document.Blocks.Count; i++) sb.AppendLine(i.ToString());
            // Ako LineTextBox nije null postavljamo sadržaj objekta sb u LineTextBox
            if (LineTextBox != null)
                new TextRange(LineTextBox.Document.ContentStart, LineTextBox.Document.ContentEnd).Text = sb.ToString();
        }

        
        
        
        private void MainTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Ako je pritisnuta tipka Tab
            if (e.Key == Key.Tab)
            {
                // Ako je paragraf na kojem se nalazi kursor null program izlazi iz metode
                if (MainTextBox.CaretPosition.Paragraph == null)
                    return;
                // Indeks, odnosno udaljenost kursora od početka paragrafa ili linije
                var start =
                    MainTextBox.CaretPosition.Paragraph.ContentStart.GetOffsetToPosition(MainTextBox.CaretPosition);

                // Ako korisnik koristi razmake pri pritisku tipke tab
                if (Properties.Settings.Default.UseSpaces)
                {
                    // Na mjestu gdje se nalazi kursor program ubacuje praznih mjesta 
                    // koliko je korisnik odabrao (svojstvo TabSize)
                    MainTextBox.CaretPosition.InsertTextInRun(new string(' ', TabSize));

                    // Pošto metoda InsertTextInRun ne pomiče kursor to radi ovaj kod
                    // U slučaju da je nova pozicija null, pozicija ostaje ista
                    MainTextBox.CaretPosition =
                        MainTextBox.CaretPosition.Paragraph.ContentStart.GetPositionAtOffset(start + TabSize) ??
                        MainTextBox.CaretPosition;
                }
                // Ako korisnik koristi Tab
                else
                {
                    // Na mjestu gdje se nalazi kursor program ubacuje jedan tab
                    MainTextBox.CaretPosition.InsertTextInRun("\t");

                    // Pomičemo kursor za jedno mjesto jer je tab veličine 1
                    MainTextBox.CaretPosition =
                        MainTextBox.CaretPosition.Paragraph.ContentStart.GetPositionAtOffset(start + 1);
                }
                // Označujemo da je event izvršen
                e.Handled = true;
            }
        }

        
        
        
        public void SaveFile()
        {
            // Ako ne postoji lokacija za pohranu, program baca grešku
            if (!HasSaveLocation)
                throw new InvalidSaveLocationException();
            // Ako postoji, koristeći StreamWriter program sprema sadržaj datoteke
            using var sw = new StreamWriter(DocumentPath);
            sw.Write(Text);
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            // Označavanje da je event izvršen
            e.Handled = true;
        }

        
        
        
        private void LanguageComboBox_OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                // Privremeno uklanjanje metode iz eventa TextChanged
                MainTextBox.TextChanged -= MainTextBox_TextChanged;
                // Poziv metode za isticanje
                HighlightRange();
                // Poziv metode za isticanje komentara
                HighlightCommentsInRange();
                // Vraćanje metode u event TextChanged
                MainTextBox.TextChanged += MainTextBox_TextChanged;

            }
            catch
            {
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Atribut NotifyPropertyChangedInvocator naznačuje da je ovo metoda eventa
        [NotifyPropertyChangedInvocator]
        // Atribut CallerMemberName automatski upisuje ime svojstva metodu pozovemo iz 
        // svojstva
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Metodom Invoke se poziva event sa argumentima trenutnog objekta 
            // (pošiljatelja) i sa argumentima eventa koji sadrži ime svojstva
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MainTextBox_OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            // Početak linije gdje na kojoj se početak odabira nalazi
            var lineStart = MainTextBox.Selection.Start.GetLineStartPosition(0);
            // Broj linije možemo dobiti tako da izračunamo razmak između početka
            // linije i mjesta trenutnog odabira
            int column = lineStart.GetOffsetAtTextPointer(MainTextBox.Selection.Start);
            // Metoda GetLineStartPosition vraća TextPointer na početak linije relativne 
            // trenutnoj liniji. Out vrijednost vraća za koliko linija se TextPointer 
            // zapravo pomaknuo
            MainTextBox.Selection.Start.GetLineStartPosition(int.MinValue, out int linesMoved);
            // linesMoved će biti negativan pa ga trebamo invertirati
            // I broj linije i broj retka kreću od nule pa dodajemo 1
            LineColumnLabel.Content = $"ln: {-linesMoved + 1}, col: {column + 1}";

            // Ako je odabran neki tekst također izračunavamo koordinate kraja odabira
            if (MainTextBox.Selection.End != MainTextBox.Selection.Start)
            {
                lineStart = MainTextBox.Selection.End.GetLineStartPosition(0);

                column = lineStart.GetOffsetAtTextPointer(MainTextBox.Selection.End);

                MainTextBox.Selection.End.GetLineStartPosition(int.MinValue, out linesMoved);
                // Rezultat pridodajemo prošlom sadržaju labele
                // U labelu isto tako dodajemo broj odabranih znakova
                LineColumnLabel.Content += $" – ln: {-linesMoved + 1}, col: {column + 1} (selected {MainTextBox.Selection.Text.Length} chars)";
            }
        }

    }
}
