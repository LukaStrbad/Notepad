using System;
using System.Windows;

namespace NotepadCore
{
    /// <summary>
    ///     Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private bool _useSpaces = true;

        private FontWindow fontDialog = new FontWindow();

        public SettingsWindow()
        {
            InitializeComponent();

            var userSettings = Settings.UserSettings.Create();

            SpacesCheckBox.IsChecked = true; // TODO: implement storage

            TabSizeTextBox.Text = userSettings.TabSize.ToString();

            FontInfo.Content = $"Font: {userSettings.EditorFontFamily}, {userSettings.EditorFontSize}";

            ShowLineNumbersCheckBox.IsChecked = Properties.Settings.Default.ShowLineNumbers;

            SpacesCheckBox.IsChecked = Properties.Settings.Default.UseSpaces;
        }

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
            }
        }

        private void SpacesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UseSpaces = true;
        }

        private void SpacesCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UseSpaces = false;
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
            var fontFamily = Convert.ToString(fontDialog.FontChooseListBox.SelectedItem);
            var fontSize = Convert.ToInt32(fontDialog.FontSizeChooseListBox.SelectedItem);

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
            if (fontDialog == null)
                fontDialog = new FontWindow();
            fontDialog.ShowDialog();

            FontInfo.Content = $"Font: {fontDialog.fontFamily}, {fontDialog.fontSize}";
        }
    }
}