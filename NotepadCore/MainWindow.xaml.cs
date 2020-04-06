using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private int _newFileNumber = 1;

        public MainWindow()
        {
            InitializeComponent();

            var userSettings = UserSettings.Create();

            // if there are files, load them
            if (userSettings.Editors.Length != 0)
            {
                foreach (var i in userSettings.Editors)
                    try
                    {
                        Tabs.Items.Insert(Tabs.Items.Count - 1, new TabItem
                        {
                            Content = new TextEditor(i.FilePath) { FileLanguage = i.HighlightingLanguage },
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
                Tabs.Items.Insert(0, EmptyTab);
                Tabs.SelectedIndex = 0;
            }

            // TODO: remove
            // Changes the font according to settings
            ChangeFont();

            // Restore previous window state
            Left = Properties.Settings.Default.LeftWindowPosition;
            Top = Properties.Settings.Default.TopWindowPosition;
            Width = Properties.Settings.Default.WindowWidth;
            Height = Properties.Settings.Default.WindowHeight;
        }

        public TextEditor CurrentTextEditor => (TextEditor)Tabs.SelectedContent;

        /// <summary>
        ///     Writes the text from MainTextBox when the window closes
        /// </summary>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var userSettings = UserSettings.Create();

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
            userSettings.RemoveInvalidFilePaths();
            userSettings.Save();

            // Save window state
            Properties.Settings.Default.LeftWindowPosition = Left;
            Properties.Settings.Default.TopWindowPosition = Top;
            Properties.Settings.Default.WindowWidth = Width;
            Properties.Settings.Default.WindowHeight = Height;
            Properties.Settings.Default.Save();

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

            if (!string.IsNullOrEmpty(newDialog.FileName))
            {
                // Adds new file path
                userSettings.AddFiles(newDialog.FileName);

                Tabs.Items.Insert(Tabs.Items.Count - 1, new TabItem
                {
                    Content = new TextEditor(newDialog.FileName),
                    Header = new FileInfo(newDialog.FileName).Name
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
            var userSettings = UserSettings.Create();
            var openDialog = new OpenFileDialog();
            openDialog.ShowDialog();
            if (string.IsNullOrEmpty(openDialog.FileName))
                return;

            // writes the new file path to the constant Path.Document location
            userSettings.AddFiles(openDialog.FileName);

            // If the selected TextEditor is empty or contains only whitespace and doesn't have save location
            // Open a new one on the same spot
            if (string.IsNullOrWhiteSpace(CurrentTextEditor.Text) && !CurrentTextEditor.HasSaveLocation)
            {
                var item = Tabs.SelectedItem as TabItem;
                item.Content = new TextEditor(openDialog.FileName);
                item.Header = new FileInfo(openDialog.FileName).Name;
            }
            else
            {
                // else insert a new TextEditor on the end
                Tabs.Items.Insert(Tabs.Items.Count - 1, new TabItem
                {
                    Content = new TextEditor(openDialog.FileName),
                    Header = new FileInfo(openDialog.FileName).Name
                });

                Tabs.SelectedIndex = Tabs.Items.Count - 2;
            }

            userSettings.Save();
        }

        private void FileClose_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = UserSettings.Create();

            // If the file doesn't have a save location ask the user to save the file
            if (!string.IsNullOrWhiteSpace(CurrentTextEditor.Text) && !CurrentTextEditor.HasSaveLocation &&
                Tabs.Items.Count == 2)
            {
                FileSave_Click(sender, e);
                userSettings.RemoveFilePaths(CurrentTextEditor.DocumentPath);
                userSettings.Save();
                Tabs.Items[Tabs.SelectedIndex] = EmptyTab;
                Tabs.SelectedIndex = 0;
                return;
            }

            // If the file is empty and doesn't have a save location do nothing
            if (string.IsNullOrWhiteSpace(CurrentTextEditor.Text) && !CurrentTextEditor.HasSaveLocation &&
                Tabs.Items.Count == 2)
                return;

            // If there are more than 2 tabs

            // Save the file if it has a save location
            var files = userSettings.Editors.ToList();
            if (CurrentTextEditor.HasSaveLocation)
                CurrentTextEditor.SaveFile();
            // If it doesn't have a save location but has some text ask the user to save the file
            else if (!string.IsNullOrWhiteSpace(CurrentTextEditor.Text))
                FileSave_Click(sender, e);

            if (Tabs.SelectedIndex == 0)
            {
                // Remove the first tab/file to account for the negative index
                Tabs.Items.RemoveAt(0);
                files.RemoveAt(0);
            }
            else
            {
                // If it's not the first tab select the previous tab
                Tabs.SelectedIndex--; // 
                // Remove the tab that was requested to be closed
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
            var userSettings = UserSettings.Create();

            if (CurrentTextEditor.HasSaveLocation)
            {
                CurrentTextEditor.SaveFile();
            }
            else
            {
                var saveDialog = new SaveFileDialog();
                saveDialog.ShowDialog();

                if (string.IsNullOrEmpty(saveDialog.FileName))
                    return;
                CurrentTextEditor.DocumentPath =
                    saveDialog.FileName; // sets the document path to that one in save file dialog
                var paths = userSettings.Editors.ToList();
                paths.Insert(Tabs.SelectedIndex,
                    new EditorInfo(HighlightingLanguage.None, CurrentTextEditor.DocumentPath));
                userSettings.Editors = paths.ToArray();
                ((TabItem)Tabs.Items[Tabs.SelectedIndex]).Header = CurrentTextEditor.FileName;
                try
                {
                    CurrentTextEditor.SaveFile();
                }
                catch
                {
                    // ignored
                }

                userSettings.Save();
            }
        }

        /// <summary>
        ///     Saves the content of the current file to a new file
        /// </summary>
        private void FileSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = UserSettings.Create();

            if (CurrentTextEditor.HasSaveLocation)
                CurrentTextEditor.SaveFile();

            var saveDialog = new SaveFileDialog();
            saveDialog.ShowDialog();

            // If file path is empty return
            if (string.IsNullOrEmpty(saveDialog.FileName))
                return;

            // Remove the current file path from settings
            userSettings.RemoveFilePaths(CurrentTextEditor.DocumentPath);

            // Assign the new file path to the current text editor and insert the file path at a specified index
            CurrentTextEditor.DocumentPath = saveDialog.FileName;
            userSettings.AddFiles(Tabs.SelectedIndex, saveDialog.FileName);

            userSettings.Save();

            ((TabItem)Tabs.SelectedItem).Header = CurrentTextEditor.FileName;
        }


        /// <summary>
        ///     Changes the font of the two textboxes according to passed values
        /// </summary>
        public void ChangeFont()
        {
            foreach (var textEdit in GetTextEditors())
            {
                // Change font of main textbox and line textbox
                textEdit.ChangeFont();
            }
        }

        /// <summary>
        ///     Opens the find/replace dialog. Work in progress
        /// </summary>
        private void FindReplace_Click(object sender, RoutedEventArgs e)
        {
            // Exits the method if a Find window is running
            if (Application.Current.Windows.OfType<Find>().Any())
                return;

            // Show Find dialog
            new Find { Owner = this }.Show();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // Show SettingsWindow dialog
            new SettingsWindow() { Owner = this }.ShowDialog();
        }

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // if + tab is selected add a new tab
            if (Tabs.SelectedItem == TabAdd)
            {
                var userSettings = UserSettings.Create();

                Tabs.Items.Insert(Tabs.Items.Count - 1, EmptyTab);
                userSettings.AddFiles("");
                userSettings.Save();
                Tabs.SelectedIndex--;
            }
        }

        private TabItem EmptyTab => new TabItem
        {
            Content = new TextEditor { DocumentPath = "" },
            Header = $"new file {_newFileNumber++}"
        };

        public IEnumerable<TextEditor> GetTextEditors()
        {
            foreach (object item in Tabs.Items)
            {
                if ((item as TabItem)?.Content is TextEditor textEditor)
                    yield return textEditor;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Tabs.Focus();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            CurrentTextEditor.MainTextBox.Focus();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Icon made by https://www.flaticon.com/authors/smashicons from www.flaticon.com");
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.T)
            {
                Tabs.SelectedIndex = Tabs.Items.Count - 1;
                e.Handled = true;
            }
            // TODO: add this
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.Tab)
            {
                // Backward tab cycle
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    if (Tabs.SelectedIndex == 0)
                        Tabs.SelectedIndex = Tabs.Items.Count - 2;
                    else
                        Tabs.SelectedIndex--;
                }
                // Forward tab cycle
                else
                {
                    if (Tabs.SelectedIndex == Tabs.Items.Count - 2)
                        Tabs.SelectedIndex = 0;
                    else
                        Tabs.SelectedIndex++;
                }
                e.Handled = true;
            }
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(CurrentTextEditor.MainTextBox.Selection.Text);
            CurrentTextEditor.MainTextBox.Selection.Text = "";
        }
        
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(CurrentTextEditor.MainTextBox.Selection.Text);
        }
        
        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            CurrentTextEditor.MainTextBox.CaretPosition.InsertTextInRun(Clipboard.GetText());
        }
    }
}