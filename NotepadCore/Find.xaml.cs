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
            InitializeComponent();
            FindTextBox.Focus();

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
                bool caseSensitive = CaseSensitiveCheckBox.IsChecked ?? false;

                try
                {
                    if (RegExCheckBox.IsChecked ?? false)
                        return new Regex(FindTextBox.Text, caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                }
                catch
                {
                    // ignored
                }

                return new Regex(Regex.Escape(FindTextBox.Text),
                    caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
            }
        }

        private RichTextBox TextBox => ((TextEditor)MainWindow.Tabs.SelectedContent).MainTextBox;

        private void RecalculateNextMatch()
        {
            var textRange = new TextRange(TextBox.Document.ContentStart, TextBox.Document.ContentEnd);
            CurrentMatch = FindRegex.Match(textRange.Text);
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            if (FindTextBox.Text == "")
                MessageBox.Show("No text to find");
            else
                FindText();
        }

        private void SetNextMatch()
        {
            var textRange = new TextRange(TextBox.Document.ContentStart, TextBox.Document.ContentEnd);

            CurrentMatch = CurrentMatch?.NextMatch();
            if (CurrentMatch == null || !CurrentMatch.Success)
                CurrentMatch = FindRegex.Match(textRange.Text);
        }

        private void FindText()
        {
            var textRange = new TextRange(TextBox.Document.ContentStart, TextBox.Document.ContentEnd);

            // Calculate offset that is caused by new lines
            var newLines = textRange.Text.IndexesOf(Environment.NewLine);

            if (CurrentMatch == null)
                RecalculateNextMatch();

            int offset = newLines.Count(x => x < CurrentMatch.Index) * Environment.NewLine.Length;

            // Select text according to the offset
            TextBox.Selection.Select(textRange.Start.GetTextPointerAtOffset(CurrentMatch.Index - offset),
                textRange.Start.GetTextPointerAtOffset(CurrentMatch.Index - offset + CurrentMatch.Length));
            SetNextMatch();

            MainWindow.Focus();
            Focus();
        }

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentMatch == null || !CurrentMatch.Success)
                SetNextMatch();
            TextBox.Selection.Text = FindRegex.Replace(TextBox.Selection.Text, ReplaceTextBox.Text);
            FindText();
        }
    }
}