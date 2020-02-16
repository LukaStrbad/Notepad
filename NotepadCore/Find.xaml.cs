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
            int lineNumber = 0;
            for (; lineNumber < TextBox.Document.Blocks.Count; lineNumber++)
                if (new TextRange(TextBox.Document.Blocks.ElementAt(lineNumber).ContentStart,
                        TextBox.Document.Blocks.ElementAt(lineNumber).ContentEnd)
                    .Contains(TextEditor.GetTextPointAt(textRange.Start, _currentMatch.Index)))
                    break;
            TextBox.Selection.Select(TextEditor.GetTextPointAt(textRange.Start, _currentMatch.Index - lineNumber * 2),
                TextEditor.GetTextPointAt(textRange.Start,
                    _currentMatch.Index + _currentMatch.Length - lineNumber * 2));
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