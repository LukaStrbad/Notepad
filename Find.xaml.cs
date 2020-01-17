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
using Notepad.ExtensionMethods;
using System.Threading;

namespace Notepad
{
    /// <summary>
    /// Interaction logic for Find.xaml
    /// </summary>
    public partial class Find : Window
    {
        readonly MainWindow mw;
        int currentCase = 0;
        List<int> occurances;
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

        private int[] IndexArray(string input, string textToFind)
        {
            var ind = new List<int>();
            for (int i = 0; i < input.Length - textToFind.Length; i++)
            {
                if (input.Substring(i) == textToFind)
                {
                    ind.Add(i);
                }
            }
            return ind.ToArray();
        }

        private bool isFirstFind = true;

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            if (FindTextBox.Text == "") MessageBox.Show("No text to find");

            FindText(isFirstFind);

            if (isFirstFind)
                isFirstFind = false;
        }

        private void FindText(bool isFirstFind, bool changeCurrentCase = true)
        {
            if (isFirstFind)
            {
                occurances = (new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd)).Text.IndexesOf(FindTextBox.Text);
                if (changeCurrentCase)
                    currentCase = 0;
                else if (currentCase >= occurances.Count)
                    currentCase = 0;
                this.isFirstFind = true;
            }
            else
            {
                if (currentCase < occurances.Count - 1) currentCase++;
                else currentCase = 0;
            }

            TextPointer temp = null;

            if (occurances.Count > 0)
                textBox.Selection.Select(temp.GetPositionAtOffset(occurances[currentCase]), temp.GetPositionAtOffset(occurances[currentCase] + FindTextBox.Text.Length));
            //else
                //textBox.SelectionLength = 0;

            mw.Focus();
            this.Focus();
        }

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            //textBox.SelectedText = ReplaceTextBox.Text ?? "";

            FindText(true, false);
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
