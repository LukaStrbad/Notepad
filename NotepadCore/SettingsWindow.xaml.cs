using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using NotepadCore.Annotations;

namespace NotepadCore
{
    /// <summary>
    ///     Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        private bool _useSpaces = true;

        public bool UseSpaces
        {
            get => _useSpaces;
            set
            {
                _useSpaces = value;
                if (value)
                {
                    TabSizeLabel.IsEnabled = true;
                    TabSizeTextBox.IsEnabled = true;
                }
                else
                {
                    TabSizeLabel.IsEnabled = false;
                    TabSizeTextBox.IsEnabled = false;
                }
                OnPropertyChanged();
            }
        }

        private FontWindow _fontDialog;
        private FontWindow FontDialog
        {
            get => _fontDialog ??= new FontWindow {Owner = this};
            set => _fontDialog = value;
        }

        public SettingsWindow()
        {
            InitializeComponent();

            var userSettings = Settings.UserSettings.Create();

            UseSpaces = true; // TODO: implement storage
            SpacesCheckBox.DataContext = this;

            TabSizeTextBox.Text = userSettings.TabSize.ToString();

            FontInfo.Content = $"Font: {userSettings.EditorFontFamily}, {userSettings.EditorFontSize}";

            ShowLineNumbersCheckBox.IsChecked = Properties.Settings.Default.ShowLineNumbers;

            UseSpaces = Properties.Settings.Default.UseSpaces;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var userSettings = Settings.UserSettings.Create();

            // save tab size
            try
            {
                if (int.TryParse(TabSizeTextBox.Text, out var size)) userSettings.TabSize = size;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
                TabSizeTextBox.Text = userSettings.TabSize.ToString();
            }

            // save font
            var fontFamily = FontDialog.FontChooseListBox.SelectedItem.ToString();
            var fontSize = (int)FontDialog.FontSizeChooseListBox.SelectedItem;

            userSettings.EditorFontFamily = fontFamily;
            userSettings.EditorFontSize = fontSize;

            var mainWindow = Application.Current.Windows[0] as MainWindow;

            // Save ShowLineNumbers boolean
            Properties.Settings.Default.ShowLineNumbers = ShowLineNumbersCheckBox.IsChecked ?? true;

            foreach (var textEditor in mainWindow.GetTextEditors())
                textEditor.ShowLineNumbers = Properties.Settings.Default.ShowLineNumbers;

            // Save UseSpaces boolean
            Properties.Settings.Default.UseSpaces = SpacesCheckBox.IsChecked ?? true;

            userSettings.Save();

            // Font info has to be saved first
            mainWindow.ChangeFont();
        }

        private void ChangeFont_Click(object sender, RoutedEventArgs e)
        {
            FontDialog.ShowDialog();

            FontInfo.Content = $"Font: {FontDialog.fontFamily}, {FontDialog.fontSize}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}