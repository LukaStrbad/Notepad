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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Notepad.ExtensionMethods;
using System.Text.RegularExpressions;

namespace Notepad
{
    /// <summary>
    /// Interaction logic for TextEditor.xaml
    /// </summary>
    public partial class TextEditor : UserControl
    {
        private string documentPath = null;
        private int oldNumberOfLines = -1;

        public bool ShowLineNumbers
        {
            // returns boolean indicating the visibility of LineTextBox
            get => LineTextBox.Visibility == Visibility.Visible;
            set
            {
                // if line numbers should be shown
                if (value)
                {
                    // make the line LineTextBox visible
                    LineTextBox.Visibility = Visibility.Visible;
                    // change grid properties for MainTextBox
                    MainTextBox.SetValue(Grid.ColumnProperty, 1);
                    MainTextBox.SetValue(Grid.ColumnSpanProperty, 1);
                }
                // if not
                else
                {
                    // make the line LineTextBox collapsed and invisible
                    LineTextBox.Visibility = Visibility.Collapsed;
                    // change grid properties for MainTextBox
                    MainTextBox.SetValue(Grid.ColumnProperty, 0);
                    MainTextBox.SetValue(Grid.ColumnSpanProperty, 2);
                }
            }
        }

        public string DocumentPath
        {
            get => documentPath;
            set
            {
                if (File.Exists(value))
                {
                    Text = File.ReadAllText(value);
                    documentPath = value;
                }
                else
                {
                    File.Create(value);
                    documentPath = value;
                }
            }
        }
        public bool HasSaveLocation
        {
            get
            {
                if (String.IsNullOrEmpty(DocumentPath))
                    return false;
                return true;
            }
        }

        public string FileName
        {
            get => new FileInfo(DocumentPath).Name;
        }

        private int _tabSize;
        public int TabSize
        {
            get => _tabSize;
            set
            {
                if (value > 0)
                {
                    _tabSize = value;
                }
                else throw new ArgumentException("Tab size can't be less than zero or zero");
            }
        }

        public string Text
        {
            get
            {
                var textRange = new TextRange(MainTextBox.Document.ContentStart, MainTextBox.Document.ContentEnd);
                return textRange.Text;
            }
            set
            {
                //var textRange = new TextRange(MainTextBox.Document.ContentStart,
                //    MainTextBox.Document.ContentEnd);
                //textRange.Text = value ?? "";

                var regex = new Regex(String.Join("|", Keywords));

                var matches = regex.Matches(value);


            }
        }

        public TextEditor()
        {
            InitializeComponent();

            var userSettings = Settings.Create();

            MainTextBox.Focus();

            ChangeFont();

            TabSize = userSettings.TabSize;
            ShowLineNumbers = userSettings.ShowLineNumbers;
        }

        public TextEditor(string documentPath)
        {
            InitializeComponent();

            var userSettings = Settings.Create();

            this.documentPath = documentPath;

            Text = "";
            try
            {
                Text = File.ReadAllText(this.documentPath);
            }
            catch (FileNotFoundException e)
            {
                File.Create(this.documentPath);
            }

            MainTextBox.Focus();

            ChangeFont();

            TabSize = userSettings.TabSize;
            ShowLineNumbers = userSettings.ShowLineNumbers;
        }

        /// <summary>
        /// Changes the font of the two textboxes
        /// </summary>
        private void ChangeFont()
        {
            var userSettings = Settings.Create();

            var ff = new FontFamily(userSettings.EditorFontFamily);

            //change font of main textbox and line textbox
            MainTextBox.FontFamily = ff;
            MainTextBox.FontSize = userSettings.EditorFontSize;
            LineTextBox.FontFamily = ff;
            LineTextBox.FontSize = userSettings.EditorFontSize;
        }

        /// <summary>
        /// Writes the line numbers when the user control loads
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e) => WriteLineNumbers();

        /// <summary>
        /// Writes the line numbers if the new number of lines if different than the old one
        /// </summary>
        private void MainTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int lineCount = new TextRange(MainTextBox.Document.ContentStart, MainTextBox.Document.ContentEnd).Text.Count(c => c == '\n');
            if (oldNumberOfLines != lineCount)
            {
                WriteLineNumbers();
                oldNumberOfLines = lineCount;
            }

            if (!highlightingRunning)
                SyntaxHighlighting();

            MainTextBox.SelectionTextBrush = Brushes.Blue;
        }

        bool highlightingRunning = false;
        public void SyntaxHighlighting()
        {
            highlightingRunning = true;
            var pattern = new Regex($"({String.Join("|", Keywords)})");

            var ptr = MainTextBox.CaretPosition;

            //var matches = pattern.Matches(Text);

            //for (int i = matches.Count - 1; i >= 0; i--)
            //{
            //    MainTextBox.Selection.Select(ptr.GetPositionAtOffset(matches[i].Index) ?? ptr, ptr.GetPositionAtOffset(matches[i].Length) ?? ptr);
            //    MainTextBox.Selection.Text = "";
            //    MainTextBox.Document.Blocks.Add(new Paragraph(new Run()
            //    {
            //        Text = matches[i].Value,
            //        Foreground = Brushes.Blue
            //    }));
            //}

            

            MainTextBox.CaretPosition = ptr;
            highlightingRunning = false;
        }

        private readonly string[] Keywords = new string[] { "using", "public", "static", "private", "class", "void" };


        private void ColorTextBox(int startIndex, int endIndex, SolidColorBrush color)
        {
            var ptr = MainTextBox.CaretPosition;
            var range = new TextRange(ptr.GetPositionAtOffset(startIndex) ?? ptr, ptr.GetPositionAtOffset(endIndex) ?? ptr);
            range.ApplyPropertyValue(TextElement.ForegroundProperty, color);
        }

        private void MainTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int lineCount = new TextRange(MainTextBox.Document.ContentStart, MainTextBox.Document.ContentEnd).Text.Count(c => c == '\n');

            if (oldNumberOfLines != lineCount)
            {
                WriteLineNumbers();
                oldNumberOfLines = lineCount;
            }
        }

        /// <summary>
        /// Synchronizes the scroll of the two textboxes
        /// </summary>
        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            LineTextBox.ScrollToVerticalOffset(MainTextBox.VerticalOffset);
        }

        /// <summary>
        /// Writes teh line numbers to the LineTextBox
        /// </summary>
        private void WriteLineNumbers()
        {
            var sb = new StringBuilder();

            for (int i = 1; i <= Text.Count(c => c == '\n'); i++)
            {
                sb.AppendLine(i.ToString());
            }

            if (LineTextBox != null)
                new TextRange(LineTextBox.Document.ContentStart, LineTextBox.Document.ContentEnd).Text = sb.ToString();
        }

        /// <summary>
        /// For custom tab size
        /// </summary>
        private void MainTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                TextPointer ptr = MainTextBox.CaretPosition;
                int start = ptr.GetOffsetToPosition(ptr);

                // If UseSpaces is true insert TabSize amount of spaces
                if (Settings.Create().UseSpaces)
                {
                    MainTextBox.CaretPosition.InsertTextInRun(new string(' ', TabSize));

                    MainTextBox.CaretPosition = ptr.GetPositionAtOffset(start + TabSize) ?? MainTextBox.CaretPosition;
                    //Text = Text.Insert(, new string(' ', TabSize));
                    //MainTextBox.CaretIndex = carretIndex + TabSize;
                }
                // Else insert a tab
                else
                {
                    MainTextBox.CaretPosition.InsertTextInRun("\t");

                    MainTextBox.CaretPosition = ptr.GetPositionAtOffset(start + 1);
                    //Text = Text.Insert(carretIndex, "\t");
                    //MainTextBox.CaretIndex = carretIndex + 1;
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Saves the MainTextBox text to 
        /// </summary
        public void SaveFile()
        {
            if (String.IsNullOrEmpty(documentPath))
                throw new InvalidSaveLocation();
            File.WriteAllText(documentPath, Text);
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}