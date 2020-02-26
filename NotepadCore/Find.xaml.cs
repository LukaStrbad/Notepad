using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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

            int lineNumber = 0;
            // Calculate line number of current match
            for (; lineNumber < TextBox.Document.Blocks.Count; lineNumber++)
            {
                if (new TextRange(TextBox.Document.Blocks.ElementAt(lineNumber).ContentStart,
                        TextBox.Document.Blocks.ElementAt(lineNumber).ContentEnd)
                    .Contains(TextEditor.GetTextPointAt(textRange.Start, _currentMatch.Index)))
                    break;
            }

            TextBox.Selection.Select(TextEditor.GetTextPointAt(textRange.Start, _currentMatch.Index - lineNumber * 2),
                TextEditor.GetTextPointAt(textRange.Start,
                    _currentMatch.Index + _currentMatch.Length - lineNumber * 2));
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