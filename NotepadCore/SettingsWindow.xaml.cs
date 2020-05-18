using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using NotepadCore.Annotations;

namespace NotepadCore
{
    
    
    
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        // Varijabla za spremanje stanja svojstva
        private bool _useSpaces = true;

        public bool UseSpaces
        {
            // Pri čitanju vrijednosti, program vraća varijablu _useSpaces
            get => _useSpaces;
            set
            {
                // Postavljanje vrijednosti u varijablu
                _useSpaces = value;
                // Ako je vrijednost true, omogućuju se kontrole TabSizeLabel i 
                // TabSizeTextBox
                if (value)
                {
                    TabSizeLabel.IsEnabled = true;
                    TabSizeTextBox.IsEnabled = true;
                }
                // Ako je false, onemogućuju se kontrole
                else
                {
                    TabSizeLabel.IsEnabled = false;
                    TabSizeTextBox.IsEnabled = false;
                }

                // Pozivanje metode koja govori da se ovo svojstvo promijenilo
                OnPropertyChanged();
            }
        }

        // Varijabla za spremanje svojstva
        private FontWindow _fontDialog;

        private FontWindow FontDialog
        {
            // Ako je pri čitanju svojstva _fontDialog null, objekt se inicijalizira
            // Ako _fontDialog null, svojstva vraća tu instancu
            get => _fontDialog ??= new FontWindow {Owner = this};
            // Postavljanje varijable _fontDialog u neku vrijednost
            set => _fontDialog = value;
        }

        public SettingsWindow()
        {
            // Inicijalizacija komponenti
            InitializeComponent();
            // Stvaranje instance korisničkih postavki
            var userSettings = Settings.UserSettings.Create();
            // Postavljanje konteksta podataka za SpacesCheckBox na ovaj objekt
            SpacesCheckBox.DataContext = this;
            // Program učitava veličinu tab-a u textbox iz postavki
            TabSizeTextBox.Text = userSettings.TabSize.ToString();
            // Mijenjanje sadržaj labele u ime fonta i veličinu fonta
            FontInfo.Content = $"Font: {userSettings.EditorFontFamily}, {userSettings.EditorFontSize}";
            // Ako se ispisuju brojevi linija, program označava pripadajući checkbox
            ShowLineNumbersCheckBox.IsChecked = Properties.Settings.Default.ShowLineNumbers;
            // Ako se ispisuje razmaci umjesto tab-ova program označava pripadajući 
            // checkbox
            SpacesCheckBox.IsChecked = Properties.Settings.Default.UseSpaces;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Stvaranje instance korisničkih postavki
            var userSettings = Settings.UserSettings.Create();

            // Spremanje veličine razmaka pri pritisku tab-a
            if (int.TryParse(TabSizeTextBox.Text, out var size))
                userSettings.TabSize = size;

            // Čitanje obitelji fonta iz ListBox-a
            var fontFamily = FontDialog.FontChooseListBox.SelectedItem.ToString();
            // Čitanje veličine fonta iz ListBox-a
            var fontSize = (int) FontDialog.FontSizeChooseListBox.SelectedItem;
            // Spremanje informacija o fontu
            userSettings.EditorFontFamily = fontFamily;
            userSettings.EditorFontSize = fontSize;

            // Referenciranje instance glavnog prozora
            var mainWindow = Application.Current.Windows[0] as MainWindow;

            // Spremanje opcije za prikazivanje brojeva linija
            // Svojstvo IsChecked ima 3 stanja (true, false, null) te zbog toga ako je  
            // vrijednost null, program sprema vrijednost true
            Properties.Settings.Default.ShowLineNumbers = ShowLineNumbersCheckBox.IsChecked ?? true;
            // Program prolazi kroz svaki tab i primijenjuje postavku prikazivanja brojeva 
            // linija
            foreach (var textEditor in mainWindow.GetTextEditors())
                textEditor.ShowLineNumbers = Properties.Settings.Default.ShowLineNumbers;

            // Spremanje opcije za ubacivanje razmaka umjesto tab-ova
            Properties.Settings.Default.UseSpaces = SpacesCheckBox.IsChecked ?? true;

            // Spremanje korisničkih postavki
            userSettings.Save();

            // Promjena fonta
            mainWindow.ChangeFont();
        }

        private void ChangeFont_Click(object sender, RoutedEventArgs e)
        {
            // Prikazivanje dijaloga
            FontDialog.ShowDialog();
            // Kada korisnik zatvori dijalog, ažurirati će se labela FontInfo
            FontInfo.Content = $"Font: {FontDialog.ChosenFontFamily}, {FontDialog.ChosenFontSize}";
        }

        // Event za događaje mijenjanja svojstva
        public event PropertyChangedEventHandler PropertyChanged;

        // Metoda koja poziva event
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Pozivanje eventa PropertyChanged sa argumentima trenutnog objekta i 
            // argumentima eventa
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}