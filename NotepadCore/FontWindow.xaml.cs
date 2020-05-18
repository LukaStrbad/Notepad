using System;
using System.Windows;

namespace NotepadCore
{
    /// <summary>
    ///     Interaction logic for Font.xaml
    /// </summary>
    public partial class FontWindow : Window
    {
        public string ChosenFontFamily { get; private set; }
        public int ChosenFontSize { get; private set; }

        public FontWindow()
        {
            // Inicijalizacija komponenti prozora
            InitializeComponent();

            // Stvaranje instance korisničkih postavki
            var userSettings = Settings.UserSettings.Create();

            // Prolazimo kroz petlju za vrijednosti od 8 do 96
            for (var i = 8; i <= 96; i++) 
                // Svaki broj dodajemo u listu veličina fontova
                FontSizeChooseListBox.Items.Add(i);
            // Program odabire font na listi prema onome koji se nalazi u postavkama
            FontChooseListBox.SelectedItem = new System.Windows.Media.FontFamily(userSettings.EditorFontFamily);
            // Program odabire veličinu fonta prema onoj koja se nalazi u postavkama
            FontSizeChooseListBox.SelectedItem =
                userSettings.EditorFontSize;

            // Odabrane postavke pomaknemo u pregledan dio lista
            FontChooseListBox.ScrollIntoView(new System.Windows.Media.FontFamily(userSettings.EditorFontFamily));
            FontSizeChooseListBox.ScrollIntoView(userSettings.EditorFontSize);
        }

        private void FontOKButton_Click(object sender, RoutedEventArgs e)
        {
            // Spremanje obitelji fonta u svojstvo
            ChosenFontFamily = FontChooseListBox.SelectedItem.ToString();
            // Spremanje veličine fonta u svojstvo
            ChosenFontSize = (int)FontSizeChooseListBox.SelectedItem;
            // Skrivanje prozora
            Hide();
        }
    }
}