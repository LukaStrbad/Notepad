using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using NotepadCore.ExtensionMethods;

namespace NotepadCore
{
    /// <summary>
    ///     Interaction logic for Find.xaml
    /// </summary>
    public partial class Find : Window
    {
        private MainWindow MainWindow => Application.Current.Windows[0] as MainWindow;
        private Match CurrentMatch { get; set; }

        public Find()
        {
            // Inicijalizacija komponenti
            InitializeComponent();
            // Fokusiranje textbox-a za unos pojma za pretraživanje
            FindTextBox.Focus();

            // Funkcija RecalculateNextMatch se poziva ako se dogodi jedan od sljedećih 
            // eventova: Promjena pojma za pretraživanje ili označavanje okvira za 
            // korištenje regex-a ili okvira za ignoriranje malih i velikih slova
            FindTextBox.TextChanged += (sender, args) => RecalculateNextMatch();
            RegExCheckBox.Checked += (sender, args) => RecalculateNextMatch();
            RegExCheckBox.Unchecked += (sender, args) => RecalculateNextMatch();
            CaseSensitiveCheckBox.Checked += (sender, args) => RecalculateNextMatch();
            CaseSensitiveCheckBox.Unchecked += (sender, args) => RecalculateNextMatch();
        }

        private Regex FindRegex
        {
            get
            {
                // Varijabla za spremanje stanja razlikuju li se velika i mala slova
                // u pretraživanju
                bool caseSensitive = CaseSensitiveCheckBox.IsChecked ?? false;

                try
                {
                    // Ako je odabrana opcija za pretraživanje sa regex-om svojstvo vraća
                    // novi Regex objekt sa zadanim uzorkom
                    // Ukoliko korisnik ne želi razlikovati velika i mala slova, odabrana
                    // je opcija RegexOptions.IgnoreCase 
                    if (RegExCheckBox.IsChecked ?? false)
                        return new Regex(FindTextBox.Text, caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                }
                catch
                {
                }

                // U slučaju da se ne koristi regex, svojstvo vraća Regex sa escape-anim
                // uzorkom
                return new Regex(Regex.Escape(FindTextBox.Text),
                    caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
            }
        }

        private RichTextBox TextBox => ((TextEditor)MainWindow.Tabs.SelectedContent).MainTextBox;

        private void RecalculateNextMatch()
        {
            // Stvaranje novog TextRange objekta koji se proteže kroz cijeli dokument
            var textRange = new TextRange(TextBox.Document.ContentStart, TextBox.Document.ContentEnd);
            // Računanje trenutnog pogotka pomoću metode Match
            CurrentMatch = FindRegex.Match(textRange.Text);
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            // Ako je tekst za pretraživanje prazan javljamo korisniku
            if (FindTextBox.Text == "")
                MessageBox.Show("No text to find");
            // Ako nije, pozivamo metodu za pronalaženje teksta
            else
                FindText();
        }

        private void SetNextMatch()
        {
            // Kreiranje TextRange objekta koji se proteže kroz cijeli dokument
            var textRange = new TextRange(TextBox.Document.ContentStart, TextBox.Document.ContentEnd);

            // Računanje sljedećeg pogotka koji može biti null ako nema instance objekta
            CurrentMatch = CurrentMatch?.NextMatch();
            // Ako je CurrentMatch null ili neuspješan računamo pogodak od početka teksta
            if (CurrentMatch == null || !CurrentMatch.Success)
                CurrentMatch = FindRegex.Match(textRange.Text);
        }

        private void FindText()
        {
            // Kreiranje objekta tipa TextRange koji se proteže kroz cijeli tekst
            var textRange = new TextRange(TextBox.Document.ContentStart, TextBox.Document.ContentEnd);

            // Računanje indeksa svake nove linije
            var newLines = textRange.Text.IndexesOf(Environment.NewLine);

            // Ako je CurrentMatch null, program računa novi pogodak
            if (CurrentMatch == null)
                RecalculateNextMatch();

            // Računanje pomaka uzrokovanog novim linijama
            // Metoda Count broji koliko elemenata u kolekciji newLines je manje o indeksa
            // trenutnog pogotka
            int offset = newLines.Count(x => x < CurrentMatch.Index) * Environment.NewLine.Length;

            // Odabire tekst s obzirom na pomak
            TextBox.Selection.Select(textRange.Start.GetTextPointerAtOffset(CurrentMatch.Index - offset),
                textRange.Start.GetTextPointerAtOffset(CurrentMatch.Index - offset + CurrentMatch.Length));
            // Postavljanje sljedećeg pogotka
            SetNextMatch();
	
            // Fokusiranjem na glavni prozor pa opet na ovaj prozor postižemo to da će 
            // tekst ostati odabran na glavnom prozoru
            MainWindow.Focus();
            Focus();
        }

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            // Ako je CurrentMatch null ili nije uspješan, program ga računa ponovo
            if (CurrentMatch == null || !CurrentMatch.Success)
                SetNextMatch();
            // Zamjena teksta
            TextBox.Selection.Text = FindRegex.Replace(TextBox.Selection.Text, ReplaceTextBox.Text);
            // Pronalaženje sljedećeg pogotka
            FindText();
        }

    }
}