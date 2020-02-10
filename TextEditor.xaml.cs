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
using System.Threading;

namespace Notepad
{
    /// <summary>
    /// Interaction logic for TextEditor.xaml
    /// </summary>
    public partial class TextEditor : UserControl
    {
        private string _documentPath = null;
        private int _oldNumberOfLines = -1;
        private Task _highlighTask;
        private CancellationTokenSource _cts;
        private List<Paragraph> _changedLines = new List<Paragraph>();

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
            get => _documentPath;
            set
            {
                if (File.Exists(value))
                {
                    Text = File.ReadAllText(value);
                    _documentPath = value;
                }
                else
                {
                    File.Create(value);
                    _documentPath = value;
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

                MainTextBox.Document.Blocks.Clear();
                MainTextBox.Document.Blocks.AddRange(HighlightLines(value.TrimEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None)));
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

            _cts = new CancellationTokenSource();
        }

        public TextEditor(string documentPath)
        {
            InitializeComponent();

            var userSettings = Settings.Create();

            this._documentPath = documentPath;

            Text = "";
            try
            {
                Text = File.ReadAllText(this._documentPath);
            }
            catch (FileNotFoundException e)
            {
                File.Create(this._documentPath);
            }

            MainTextBox.Focus();

            ChangeFont();

            TabSize = userSettings.TabSize;
            ShowLineNumbers = userSettings.ShowLineNumbers;

            _cts = new CancellationTokenSource();
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

            int lineCount = MainTextBox.Document.Blocks.Count;
            if (_oldNumberOfLines != lineCount)
            {
                WriteLineNumbers();
                _oldNumberOfLines = lineCount;
            }

            if (!_changedLines.Contains(MainTextBox.CaretPosition.Paragraph))
                _changedLines.Add(MainTextBox.CaretPosition.Paragraph);

            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = new CancellationTokenSource();
                //_highlighTask = new Task(() => HighlightMissingLines(_cts.Token));
                _highlighTask?.Dispose();
                _highlighTask = new Task(() => HighlightParagraph(_cts.Token));
                _highlighTask.Start();
            }
        }

        private async void HighlightMissingLines(CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Delay(50, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                Dispatcher.Invoke(() =>
                {
                    var par = MainTextBox.CaretPosition.Paragraph;
                    int caretIndex = par.ContentStart.GetOffsetToPosition(MainTextBox.CaretPosition);


                    string text = new TextRange(par.ContentStart, par.ContentEnd).Text;
                    var paragraph = HighlightLine(text, caretIndex, out int newCaretIndex);

                    cancellationToken.ThrowIfCancellationRequested();

                    MainTextBox.TextChanged -= MainTextBox_TextChanged;

                    par.Inlines.Clear();
                    par.Inlines.AddRange(paragraph.Inlines.ToArray());

                    //using (var ms = new MemoryStream())
                    //{
                    //    new TextRange(par.ContentStart, par.ContentEnd).Save(ms, DataFormats.Rtf);
                    //    MessageBox.Show(ASCIIEncoding.Default.GetString(ms.ToArray()));
                    //}



                    MainTextBox.CaretPosition = par.ContentStart.GetPositionAtOffset(caretIndex - (newCaretIndex - caretIndex));
                    MainTextBox.TextChanged += MainTextBox_TextChanged;
                }, System.Windows.Threading.DispatcherPriority.Normal, cancellationToken);
            }
            catch (Exception e) { }
        }

        public List<Paragraph> HighlightLines(params string[] lines)
        {
            var tempList = new List<Paragraph>(lines.Length);
            foreach (var line in lines)
            {
                tempList.Add(HighlightLine(line));
            }
            return tempList;
        }

        private readonly string[] Keywords = new string[] { "using", "public", "static", "private", "class", "void", "string", "int", "double", "float", "long", "namespace" };
        private Paragraph HighlightLine(string line)
        {
            return HighlightLine(line, 0, out _);
        }


        private static TextPointer GetTextPointAt(TextPointer from, int pos)
        {
            TextPointer ret = from;
            int i = 0;

            while ((i < pos) && (ret != null))
            {
                if ((ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text) || (ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.None))
                    i++;

                if (ret.GetPositionAtOffset(1, LogicalDirection.Forward) == null)
                    return ret;

                ret = ret.GetPositionAtOffset(1, LogicalDirection.Forward);
            }

            return ret;
        }

        private async void HighlightParagraph(CancellationToken cancellationToken = default)
        {
            MainTextBox.TextChanged -= MainTextBox_TextChanged;
            try
            {
                await Task.Delay(1000, cancellationToken);
            }
            catch { }
            Dispatcher.Invoke(() =>
            {
                try
                {
                    var pattern = new Regex(String.Join("|", Keywords));
                    var textRange = new TextRange(MainTextBox.CaretPosition.Paragraph.ContentStart, MainTextBox.CaretPosition.Paragraph.ContentEnd);
                    textRange.ClearAllProperties();
                    var matches = pattern.Matches(textRange.Text);

                    cancellationToken.ThrowIfCancellationRequested();

                    foreach (Match match in matches)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        Dispatcher.Invoke(() =>
                        {
                            new TextRange(GetTextPointAt(textRange.Start, match.Index), GetTextPointAt(textRange.Start, match.Index + match.Length)).ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                        });
                    }
                }
                catch (Exception ex)
                { }
            });
            MainTextBox.TextChanged += MainTextBox_TextChanged;
        }

        private Paragraph HighlightLine(string line, int caretIndex, out int newCaretIndex)
        {
            var pattern = new Regex(String.Join("|", Keywords)); // Pattern for keywords
            var paragraph = new Paragraph(); // A paragraph where highlighted lines will be added
            var sb = new StringBuilder();
            var matches = pattern.Matches(line);
            var lastRun = new Run();
            newCaretIndex = caretIndex;

            //int i = 0;
            //while (i < line.Length)
            //{
            //    if (matches.Select(x => x.Index).Contains(i)) // If index is in matches
            //    {
            //        if (sb.Length > 0) // If sb has text
            //        {
            //            lastRun = new Run(sb.ToString());
            //            paragraph.Inlines.Add(lastRun); // Add new nonhighlighted text to paragraph
            //            if (i < caretIndex)
            //                newCaretIndex += 2; // Opening and closing tags count as characters
            //        }
            //        sb.Clear();
            //        // Add new highlighted text
            //        int tempLength = matches.First(x => x.Index == i).Length;

            //        lastRun = new Run(line.Substring(i, tempLength))
            //        {
            //            Foreground = Brushes.Blue
            //        };
            //        paragraph.Inlines.Add(lastRun);
            //        //if (i < caretIndex)
            //        //{
            //        //    if (caretIndex >= i && caretIndex < i + tempLength)
            //        //        newCaretIndex += 2;
            //        //    else
            //        //        newCaretIndex++;
            //        //}
            //        i += tempLength;
            //    }
            //    else
            //    {
            //        sb.Append(line[i]);
            //        i++;
            //    }
            //}
            //if (sb.Length > 0)
            //{
            //    lastRun = new Run(sb.ToString());
            //    paragraph.Inlines.Add(lastRun);
            //}

            //try
            //{
            //    var matchAfterCaret = matches.First(match => caretIndex >= match.Index);
            //    newCaretIndex = paragraph.ContentStart.GetOffsetToPosition(paragraph.Inlines.First(inline => inline == lastRun).ContentStart) + (caretIndex - matchAfterCaret.Index + 1);
            //    using (var ms = new MemoryStream())
            //    {
            //        new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Save(ms, DataFormats.Xaml);
            //        //MessageBox.Show(ASCIIEncoding.Default.GetString(ms.ToArray()));
            //    }
            //}
            //catch
            //{
            //    newCaretIndex = caretIndex;
            //}

            return paragraph;
        }

        private void MainTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int lineCount = new TextRange(MainTextBox.Document.ContentStart, MainTextBox.Document.ContentEnd).Text.Count(c => c == '\n');

            if (_oldNumberOfLines != lineCount)
            {
                WriteLineNumbers();
                _oldNumberOfLines = lineCount;
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

            for (int i = 1; i <= MainTextBox.Document.Blocks.Count; i++)
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
                int start = MainTextBox.CaretPosition.Paragraph.ContentStart.GetOffsetToPosition(MainTextBox.CaretPosition);

                // If UseSpaces is true insert TabSize amount of spaces
                if (Settings.Create().UseSpaces)
                {
                    MainTextBox.CaretPosition.InsertTextInRun(new string(' ', TabSize));

                    MainTextBox.CaretPosition = MainTextBox.CaretPosition.Paragraph.ContentStart.GetPositionAtOffset(start + TabSize) ?? MainTextBox.CaretPosition;
                    //Text = Text.Insert(, new string(' ', TabSize));
                    //MainTextBox.CaretIndex = carretIndex + TabSize;
                }
                // Else insert a tab
                else
                {
                    MainTextBox.CaretPosition.InsertTextInRun("\t");

                    MainTextBox.CaretPosition = MainTextBox.CaretPosition.Paragraph.ContentStart.GetPositionAtOffset(start + 1);
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
            if (String.IsNullOrEmpty(_documentPath))
                throw new InvalidSaveLocation();
            File.WriteAllText(_documentPath, Text);
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}