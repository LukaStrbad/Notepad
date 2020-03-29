using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using NotepadCore.Annotations;
using NotepadCore.Exceptions;
using NotepadCore.ExtensionMethods;
using NotepadCore.SyntaxHighlighters;

namespace NotepadCore
{
    /// <summary>
    ///     Interaction logic for TextEditor.xaml
    /// </summary>
    public partial class TextEditor : UserControl, INotifyPropertyChanged
    {
        private string _documentPath;
        private int _oldNumberOfLines = -1;
        private HighlightingLanguage _fileLanguage;
        private int _tabSize;

        public HighlightingLanguage FileLanguage
        {
            get => _fileLanguage;
            set
            {
                // Save new FileLanguage value and notify that property has been changed
                _fileLanguage = value;
                OnPropertyChanged();

                // Save FileLanguage for current editor
                var userSettings = Settings.UserSettings.Create();
                foreach (var editor in userSettings.Editors)
                {
                    if (editor.FilePath?.ToLower() == _documentPath?.ToLower() && _documentPath != null) // Find current editor
                    {
                        editor.HighlightingLanguage = value;
                        break;
                    }
                }
                userSettings.Save();
            }
        }

        public int TabSize
        {
            get => _tabSize;
            set
            {
                if (value > 0)
                    _tabSize = value;
                else throw new ArgumentException("Tab size can't be less than zero or zero");
            }
        }

        public string DocumentPath
        {
            get => _documentPath;
            set
            {
                if (File.Exists(value))
                    Text = File.ReadAllText(value);
                else
                    Text = "";

                _documentPath = value;
            }
        }

        private IHighlighter Highlighter =>
            FileLanguage switch
            {
                HighlightingLanguage.CSharp => new CSharpHighlighter(),
                HighlightingLanguage.MarkupLanguage => new MarkupHighlighter(),
                _ => new EmptyHighlighter()
            };

        public TextEditor()
        {
            InitializeComponent();
            LanguageComboBox.DataContext = this;

            var userSettings = Settings.UserSettings.Create();

            MainTextBox.Focus();

            ChangeFont();

            TabSize = userSettings.TabSize;
            ShowLineNumbers = Properties.Settings.Default.ShowLineNumbers;

            FileLanguage = HighlightingLanguage.None;
        }

        public TextEditor(string documentPath) : this()
        {
            DocumentPath = documentPath;

            if (!HasSaveLocation)
            {
                try
                {
                    File.Create(DocumentPath);
                }
                catch
                {
                }
            }
        }

        // Returns true if file exists
        public bool HasSaveLocation => File.Exists(DocumentPath);

        public string FileName
        {
            get
            {
                if (HasSaveLocation)
                    return new FileInfo(DocumentPath).Name;
                return "";
            }
        }


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

        public string Text
        {
            get
            {
                var textRange = new TextRange(MainTextBox.Document.ContentStart, MainTextBox.Document.ContentEnd);
                return textRange.Text;
            }
            set
            {
                MainTextBox.Document.Blocks.Clear();
                var paragraphs = value.Trim()
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                    .Select(a => new Paragraph(new Run(a)));
                MainTextBox.Document.Blocks.AddRange(paragraphs);

                HighlightBlocks();
            }
        }

        /// <summary>
        ///     Changes the font of the two textboxes
        /// </summary>
        public void ChangeFont()
        {
            var userSettings = Settings.UserSettings.Create();

            var fontFamily = new FontFamily(userSettings.EditorFontFamily);

            //change font of main textbox and line textbox
            MainTextBox.FontFamily = fontFamily;
            MainTextBox.FontSize = userSettings.EditorFontSize;
            LineTextBox.FontFamily = fontFamily;
            LineTextBox.FontSize = userSettings.EditorFontSize;
        }

        /// <summary>
        ///     Writes the line numbers when the user control loads
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (HasSaveLocation)
            {
                var fileInfo = new FileInfo(DocumentPath);

                FileLanguage = fileInfo.Extension switch
                {
                    ".cs" => HighlightingLanguage.CSharp,
                    _ => HighlightingLanguage.None
                };

                if (fileInfo.Extension.EndsWith("ml"))
                    FileLanguage = HighlightingLanguage.MarkupLanguage;
            }

            WriteLineNumbers();
        }

        /// <summary>
        ///     Writes the line numbers if the new number of lines if different than the old one
        /// </summary>
        private void MainTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var lineCount = MainTextBox.Document.Blocks.Count;
            // TODO: invert if
            if (_oldNumberOfLines != lineCount)
            {
                WriteLineNumbers();
                _oldNumberOfLines = lineCount;
            }

            HighlightCurrentLine();
        }

        private void HighlightCurrentLine()
        {
            if (FileLanguage == HighlightingLanguage.None)
                return;
            if (MainTextBox.CaretPosition.Paragraph == null)
                return;
            MainTextBox.TextChanged -= MainTextBox_TextChanged;

            var textRange = new TextRange(MainTextBox.CaretPosition.Paragraph.ContentStart,
                MainTextBox.CaretPosition.Paragraph.ContentEnd);
            textRange.ClearAllProperties();

            foreach (var (match, brush) in Highlighter.GetMatches(textRange))
            {
                Dispatcher?.Invoke(() =>
                {
                    new TextRange(textRange.Start.GetTextPointerAtOffset(match.Index),
                            textRange.Start.GetTextPointerAtOffset(match.Index + match.Length))
                        .ApplyPropertyValue(TextElement.ForegroundProperty, brush);
                });
            }

            MainTextBox.TextChanged += MainTextBox_TextChanged;
        }

        private void HighlightBlocks(TextPointer start = null, TextPointer end = null)
        {
            if (MainTextBox == null) return;
            MainTextBox.TextChanged -= MainTextBox_TextChanged;
            var textRange = new TextRange(start ?? MainTextBox.Document.ContentStart,
                end ?? MainTextBox.Document.ContentEnd);
            textRange.ClearAllProperties();



            foreach (var (match, brush) in Highlighter?.GetMatches(textRange))
            {
                try
                {
                    Dispatcher?.Invoke(() =>
                    {
                        new TextRange(textRange.Start.GetTextPointerAtOffset(match.Index),
                                    textRange.Start.GetTextPointerAtOffset(match.Index + match.Length))
                                .ApplyPropertyValue(TextElement.ForegroundProperty, brush);
                    });

                }
                catch { }
            }

            MainTextBox.TextChanged += MainTextBox_TextChanged;
        }


        private void MainTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var lineCount = MainTextBox.Document.Blocks.Count;

            if (_oldNumberOfLines == lineCount) return;
            WriteLineNumbers();
            _oldNumberOfLines = lineCount;
        }

        /// <summary>
        ///     Synchronizes the scroll of the two textboxes
        /// </summary>
        private void ScrollChanged(object sender, ScrollChangedEventArgs e) =>
            LineTextBox.ScrollToVerticalOffset(MainTextBox.VerticalOffset);


        /// <summary>
        ///     Writes the line numbers to the LineTextBox
        /// </summary>
        private void WriteLineNumbers()
        {
            var sb = new StringBuilder();

            for (var i = 1; i <= MainTextBox.Document.Blocks.Count; i++) sb.AppendLine(i.ToString());

            if (LineTextBox != null)
                new TextRange(LineTextBox.Document.ContentStart, LineTextBox.Document.ContentEnd).Text = sb.ToString();
        }

        /// <summary>
        ///     For custom tab size
        /// </summary>
        private void MainTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                if (MainTextBox.CaretPosition.Paragraph == null)
                    return;
                var start =
                    MainTextBox.CaretPosition.Paragraph.ContentStart.GetOffsetToPosition(MainTextBox.CaretPosition);

                // If UseSpaces is true insert TabSize amount of spaces
                if (Properties.Settings.Default.UseSpaces)
                {
                    MainTextBox.CaretPosition.InsertTextInRun(new string(' ', TabSize));

                    //MainTextBox.CaretPosition =
                    //  MainTextBox.CaretPosition.Paragraph.ContentStart.GetPositionAtOffset(start + TabSize) ??
                    //MainTextBox.CaretPosition;
                }
                // Else insert a tab
                else
                {
                    MainTextBox.CaretPosition.InsertTextInRun("\t");

                    MainTextBox.CaretPosition =
                        MainTextBox.CaretPosition.Paragraph.ContentStart.GetPositionAtOffset(start + 1);
                }
                e.Handled = true;
            }
        }

        /// <summary>
        ///     Saves the MainTextBox text to
        /// </summary>
        public void SaveFile()
        {
            if (!HasSaveLocation)
                throw new InvalidSaveLocationException();
            using var sw = new StreamWriter(DocumentPath);
            sw.Write(Text);
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Highlights the MainTextBox when changing language
        /// </summary>
        private void LanguageComboBox_OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                HighlightBlocks();
            }
            catch
            {
                // ignored
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MainTextBox_OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            // TODO: fix
            var tp1 = MainTextBox.Selection.Start.GetLineStartPosition(0);
            var tp2 = MainTextBox.Selection.Start;

            //int column = tp1.GetOffsetToPosition(tp2);
            int column = tp1.GetOffsetAtTextPointer(tp2);

            int someBigNumber = int.MaxValue;
            int lineMoved, currentLineNumber;
            MainTextBox.Selection.Start.GetLineStartPosition(-someBigNumber, out lineMoved);
            currentLineNumber = -lineMoved;


            LineColumnLabel.Content = $"ln: {currentLineNumber}, col: {column}";
        }
    }
}