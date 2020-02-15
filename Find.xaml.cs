using System;
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
        }

        private Regex FindRegex
        {
            get
            {
                if (RegExCheckBox.IsChecked ?? false)
                    return new Regex(FindTextBox.Text);
                return new Regex(Regex.Escape(FindTextBox.Text));
            }
        }

        private RichTextBox TextBox => ((TextEditor) _mw.Tabs.SelectedContent).MainTextBox;

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            if (FindTextBox.Text == "") MessageBox.Show("No text to find");

            FindText();
        }

        private void FindText()
        {
            var textRange = new TextRange(TextBox.Document.ContentStart, TextBox.Document.ContentEnd);

            if (_currentMatch == null || !_currentMatch.Success)
                _currentMatch = FindRegex.Match(textRange.Text);
            TextBox.Selection.Select(TextEditor.GetTextPointAt(textRange.Start, _currentMatch.Index),
                TextEditor.GetTextPointAt(textRange.Start, _currentMatch.Index + _currentMatch.Length));
            _currentMatch = _currentMatch.NextMatch();

            _mw.Focus();
            Focus();
        }

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            //textBox.SelectedText = ReplaceTextBox.Text ?? "";

            //FindText(true, false);
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