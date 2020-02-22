using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using NotepadCore.Exceptions;
using NotepadCore.Settings;
using NotepadCore.SyntaxHighlighters;

namespace NotepadCore
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _documentPath = "contents.txt";
        private int _newFileNumber = 1;

        public MainWindow()
        {
            InitializeComponent();

            var userSettings = Settings.UserSettings.Create();

            // if there are files, load them
            if (userSettings.Editors.Length != 0)
            {
                foreach (var i in userSettings.Editors)
                    try
                    {
                        Tabs.Items.Insert(Tabs.Items.Count - 1, new TabItem
                        {
                            Content = new TextEditor(i.FilePath) {FileLanguage = i.HighlightingLanguage},
                            Header = new FileInfo(i.FilePath).Name
                        });
                    }
                    catch
                    {
                    }

                // Select the tab that was previously selected
                Tabs.SelectedIndex = userSettings.SelectedFileIndex;
            }
            // else insert an empty tab
            else
            {
                Tabs.Items.Insert(Tabs.Items.Count - 1, new TabItem
                {
                    Content = new TextEditor(),
                    Header = $"*new file {_newFileNumber++}"
                });
                Tabs.SelectedIndex = 0;
            }

            // Changes the font according to settings
            ChangeFont();
        }

        public TextEditor CurrentTextEditor => Tabs.SelectedContent as TextEditor;

        /// <summary>
        ///     Writes the text from MainTextBox when the window closes
        /// </summary>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var userSettings = Settings.UserSettings.Create();

            // loop through Text Editors and save contents
            for (var i = 0; i < Tabs.Items.Count - 1; i++)
                try
                {
                    // saves the text from the TextEditor
                    ((Tabs.Items[i] as TabItem).Content as TextEditor).SaveFile();
                }
                catch (InvalidSaveLocationException ex)
                {
                    // select the tab without a save location and ask for a save location
                    if (!string.IsNullOrEmpty(((Tabs.Items[i] as TabItem).Content as TextEditor).Text))
                    {
                        Tabs.SelectedIndex = i;
                        FileSave_Click(null, null);
                    }
                }

            // Save the index of a currently selected tab
            userSettings.SelectedFileIndex = Tabs.SelectedIndex;
            userSettings.Save();

            Application.Current.Shutdown();
        }

        /// <summary>
        ///     Creates and opens the file
        /// </summary>
        private void FileNew_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = UserSettings.Create();

            var newDialog = new SaveFileDialog();
            newDialog.ShowDialog();
            _documentPath = newDialog.FileName;

            if (!string.IsNullOrEmpty(_documentPath))
            {
                // Adds new file path
                userSettings.AddFiles(_documentPath);

                Tabs.Items.Insert(Tabs.Items.Count - 1, new TabItem
                {
                    Content = new TextEditor(_documentPath),
                    Header = new FileInfo(_documentPath).Name
                });
                userSettings.SelectedFileIndex = userSettings.Editors.Length - 1;
                Tabs.SelectedIndex = userSettings.Editors.Length - 1;
            }

            userSettings.Save();
        }

        /// <summary>
        ///     Opens an existing file
        /// </summary>
        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = Settings.UserSettings.Create();
            var openDialog = new OpenFileDialog();
            openDialog.ShowDialog();
            if (string.IsNullOrEmpty(openDialog.FileName))
                return;
            _documentPath = openDialog.FileName;

            if (!string.IsNullOrEmpty(_documentPath))
                // writes the new file path to the constant Path.Document location
                userSettings.AddFiles(_documentPath);

            if (string.IsNullOrEmpty(CurrentTextEditor.Text))
            {
                // if the selected TextEditor is empty, open a new one on the same spot
                var item = Tabs.SelectedItem as TabItem;
                item.Content = new TextEditor(_documentPath);
                item.Header = new FileInfo(_documentPath).Name;
            }
            else
            {
                // else insert a new TextEditor on the end
                Tabs.Items.Insert(Tabs.Items.Count - 1, new TabItem
                {
                    Content = new TextEditor(_documentPath),
                    Header = new FileInfo(_documentPath).Name
                });

                Tabs.SelectedIndex = Tabs.Items.Count - 2;
            }

            userSettings.Save();
        }

        private void FileClose_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = Settings.UserSettings.Create();

            var files = userSettings.Editors.ToList();
            if (CurrentTextEditor.HasSaveLocation)
                CurrentTextEditor.SaveFile();


            if (Tabs.SelectedIndex == 0)
            {
                Tabs.Items.RemoveAt(0);
                files.RemoveAt(0);
            }
            else
            {
                Tabs.SelectedIndex--; // 
                Tabs.Items.RemoveAt(Tabs.SelectedIndex + 1);
                files.RemoveAt(Tabs.SelectedIndex + 1);
            }

            userSettings.Editors = files.ToArray();
            userSettings.Save();
        }

        /// <summary>
        ///     Saves the current file
        /// </summary>
        private void FileSave_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = Settings.UserSettings.Create();

            var textEdit = Tabs.SelectedContent as TextEditor;
            if (textEdit.HasSaveLocation)
            {
                textEdit.SaveFile();
            }
            else
            {
                var saveDialog = new SaveFileDialog();
                saveDialog.ShowDialog();

                if (string.IsNullOrEmpty(saveDialog.FileName))
                    return;
                textEdit.DocumentPath = saveDialog.FileName; // sets the document path to that one in save file dialog
                var paths = userSettings.Editors.ToList();
                paths.Insert(Tabs.SelectedIndex, new EditorInfo(HighlightingLanguage.None, textEdit.DocumentPath));
                userSettings.Editors = paths.ToArray();
                (Tabs.Items[Tabs.SelectedIndex] as TabItem).Header = textEdit.FileName;

                userSettings.Save();
            }
        }

        /// <summary>
        ///     Saves the content of the current file to a new file
        /// </summary>
        private void FileSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = Settings.UserSettings.Create();

            var textEditor = Tabs.SelectedContent as TextEditor;
            if (textEditor.HasSaveLocation) textEditor.SaveFile();

            var textEditorText = textEditor.Text;

            var saveDialog = new SaveFileDialog();
            saveDialog.ShowDialog();
            textEditor.DocumentPath = saveDialog.FileName;
            textEditor.Text = textEditorText;

            // change file paths
            var files = userSettings.Editors.ToList();
            files.RemoveAt(Tabs.SelectedIndex);
            userSettings.Editors = files.ToArray();

            userSettings.Save();

            (Tabs.SelectedItem as TabItem).Header = textEditor.FileName;
        }


        /// <summary>
        ///     Opens the ChangeFontDialog
        /// </summary>
        private void ChangeFontDialog_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = Settings.UserSettings.Create();

            var fontDialog = new FontWindow();
            fontDialog.ShowDialog();

            userSettings.EditorFontFamily = fontDialog.fontFamily;
            userSettings.EditorFontSize = fontDialog.fontSize;
            userSettings.Save();

            ChangeFont();
        }


        /// <summary>
        ///     Changes the font of the two textboxes according to passed values
        /// </summary>
        public void ChangeFont()
        {
            var userSettings = Settings.UserSettings.Create();

            foreach (var textEdit in GetTextEditors())
            {
                //change font of main textbox and line textbox
                textEdit.MainTextBox.FontFamily = new FontFamily(userSettings.EditorFontFamily);
                textEdit.MainTextBox.FontSize = userSettings.EditorFontSize;
                textEdit.LineTextBox.FontFamily = new FontFamily(userSettings.EditorFontFamily);
                textEdit.LineTextBox.FontSize = userSettings.EditorFontSize;
            }
        }

        /// <summary>
        ///     Opens the find/replace dialog. Work in progress
        /// </summary>
        private void FindReplace_Click(object sender, RoutedEventArgs e)
        {
            // TODO: add tab support
            new Find().Show();
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            //var textEdit = Tabs.SelectedContent as TextEditor;
            //Clipboard.SetDataObject(textEdit.MainTextBox.SelectedText); // copies the text to clipboard
            //textEdit.MainTextBox.Text = textEdit.MainTextBox.Text.Remove(textEdit.MainTextBox.SelectionStart, textEdit.MainTextBox.SelectionLength); // deletes the text

            var textSelection = CurrentTextEditor.MainTextBox.Selection;
            Clipboard.SetDataObject(textSelection.Text);
            textSelection.ClearAllProperties();
            textSelection.Text = "";
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            //var textEdit = Tabs.SelectedContent as TextEditor;
            //Clipboard.SetDataObject(textEdit.MainTextBox.SelectedText); // copies the text to clipboard
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            //var textEdit = Tabs.SelectedContent as TextEditor;
            //int caretIndex = textEdit.MainTextBox.CaretIndex;
            //textEdit.MainTextBox.Text = textEdit.MainTextBox.Text.Insert(textEdit.MainTextBox.CaretIndex, Clipboard.GetText()); // puts the text from the clipboard
            //textEdit.MainTextBox.CaretIndex = caretIndex + Clipboard.GetText().Length;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // show SettingsWindow
            var settingsWindow = new SettingsWindow();

            settingsWindow.ShowDialog();
        }

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // if + tab is selected add a new tab
            if (Tabs.SelectedItem == TabAdd)
            {
                Tabs.Items.Insert(Tabs.Items.Count - 1, new TabItem
                {
                    Content = new TextEditor(),
                    Header = $"*new file {_newFileNumber++}"
                });
                Tabs.SelectedIndex--;
            }
        }

        public List<TextEditor> GetTextEditors()
        {
            return (from object i in Tabs.Items
                where (i as TabItem).Content is TextEditor
                select (i as TabItem).Content as TextEditor).ToList();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Tabs.Focus();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            (Tabs.SelectedContent as TextEditor).MainTextBox.Focus();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Icon made by https://www.flaticon.com/authors/smashicons from www.flaticon.com");
            // Icon made by https://www.flaticon.com/authors/smashicons from www.flaticon.com
        }
    }
}