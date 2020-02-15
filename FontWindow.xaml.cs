using System;
using System.Windows;

namespace NotepadCore
{
    /// <summary>
    ///     Interaction logic for Font.xaml
    /// </summary>
    public partial class FontWindow : Window
    {
        public string fontFamily;
        public int fontSize;

        public FontWindow()
        {
            InitializeComponent();

            var userSettings = Settings.Create();

            // write font sizes from 8 to 96
            for (var i = 8; i <= 96; i++) FontSizeChooseListBox.Items.Add(i.ToString());

            FontChooseListBox.SelectedItem = userSettings.EditorFontFamily; // selects the fontfamily in the listbox
            FontSizeChooseListBox.SelectedItem =
                userSettings.EditorFontSize.ToString(); // selects the font size in the listbox

            FontChooseListBox.ScrollIntoView(userSettings.EditorFontFamily); // scrolls to the font in the listbox
            FontSizeChooseListBox.ScrollIntoView(userSettings.EditorFontSize
                .ToString()); // scrolls to the font size in the listbox
        }

        private void FontOKButton_Click(object sender, RoutedEventArgs e)
        {
            fontFamily = Convert.ToString(FontChooseListBox.SelectedItem);
            fontSize = Convert.ToInt32(FontSizeChooseListBox.SelectedItem);
            Hide();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            FontOKButton_Click(new object(), new RoutedEventArgs());
        }
    }
}