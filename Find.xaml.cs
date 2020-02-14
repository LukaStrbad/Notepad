using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using NotepadCore.ExtensionMethods;
using System.Threading;

namespace NotepadCore
{
    /// <summary>
    /// Interaction logic for Find.xaml
    /// </summary>
    public partial class Find : Window
    {
        private readonly MainWindow mw;
        private Match currentMatch;
        private Regex FindRegex
        {
            get
            {
                if (RegExCheckBox.IsChecked ?? false)
                    return new Regex(FindTextBox.Text);
                return new Regex(Regex.Escape(FindTextBox.Text));
            }
        }

        private RichTextBox textBox
        {
            get => (mw.Tabs.SelectedContent as TextEditor).MainTextBox;
        }

        public Find()
        {
            InitializeComponent();
            FindTextBox.Focus();
            mw = Application.Current.Windows[0] as MainWindow;
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            if (FindTextBox.Text == "") MessageBox.Show("No text to find");

            FindText();
        }

        private void FindText()
        {
            var textRange = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd);

            if (currentMatch == null || !currentMatch.Success)
                currentMatch = FindRegex.Match(textRange.Text);
            textBox.Selection.Select(TextEditor.GetTextPointAt(textRange.Start, currentMatch.Index), TextEditor.GetTextPointAt(textRange.Start, currentMatch.Index + currentMatch.Length));
            currentMatch = currentMatch.NextMatch();
            //if (isFirstFind)
            //{
            //    if (RegExCheckBox.IsChecked ?? false)
            //        occurances = new Regex(FindTextBox.Text).Matches(textRange.Text);
            //    else
            //        occurances = new Regex(Regex.Escape(FindTextBox.Text)).Matches(textRange.Text);

            //    if (changeCurrentCase)
            //        currentCase = 0;
            //    else if (currentCase >= occurances.Count)
            //        currentCase = 0;
            //    this._isFirstFind = true;
            //}
            //else
            //{
            //    if (currentCase < occurances.Count - 1) currentCase++;
            //    else currentCase = 0;
            //}

            //if (occurances.Count > 0)
            //    textBox.Selection.Select(TextEditor.GetTextPointAt(textRange.Start, occurances[currentCase].Index), TextEditor.GetTextPointAt(textRange.Start, occurances[currentCase].Index + occurances[currentCase].Length));
            //else
            //    textBox.Selection.Select(textRange.Start, textRange.Start);

            mw.Focus();
            this.Focus();
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
            this.Close();
        }
    }
}
