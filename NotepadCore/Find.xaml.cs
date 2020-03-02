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
        private readonly MainWindow _mw;
        private Match _currentMatch;

        public Find()
        {
            InitializeComponent();
            FindTextBox.Focus();
            _mw = Application.Current.Windows[0] as MainWindow;

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

        private void RecalculateNextMatch()
        {
            var textRange = new TextRange(TextBox.Document.ContentStart, TextBox.Document.ContentEnd);
            _currentMatch = FindRegex.Match(textRange.Text);
        }

        private RichTextBox TextBox => ((TextEditor) _mw.Tabs.SelectedContent).MainTextBox;

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            if (FindTextBox.Text == "") MessageBox.Show("No text to find");

            FindText();
        }

        private void SetNextMatch()
        {
            var textRange = new TextRange(TextBox.Document.ContentStart, TextBox.Document.ContentEnd);

            _currentMatch = _currentMatch?.NextMatch();
            if (_currentMatch == null || !_currentMatch.Success)
                _currentMatch = FindRegex.Match(textRange.Text);
        }

        private void FindText()
        {
            var textRange = new TextRange(TextBox.Document.ContentStart, TextBox.Document.ContentEnd);

            // Calculate offset that is caused by new lines
            var newLines = textRange.Text.IndexesOf(Environment.NewLine);
            int offset = newLines.Count(x => x < _currentMatch.Index) * Environment.NewLine.Length;

            // Select text according to the offset
            TextBox.Selection.Select(TextEditor.GetTextPointAt(textRange.Start, _currentMatch.Index - offset),
                TextEditor.GetTextPointAt(textRange.Start,
                    _currentMatch.Index - offset + _currentMatch.Length));
            SetNextMatch();

            _mw.Focus();
            Focus();
        }

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMatch == null || !_currentMatch.Success)
                SetNextMatch();
            TextBox.Selection.Text = FindRegex.Replace(TextBox.Selection.Text, ReplaceTextBox.Text);
            FindText();
        }

        private void FindTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Close();
        }
    }
}