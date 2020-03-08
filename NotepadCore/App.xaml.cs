using System.Windows;
using NotepadCore.Settings;

namespace NotepadCore
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0) // Check if there are args
            {
                var userSettings = UserSettings.Create();
                
                userSettings.AddFiles(e.Args); // Add all files to user settings
                userSettings.SelectedFileIndex = userSettings.Editors.Length - 1; // Change selected index to last added file
                userSettings.Save();
            }
        }
    }
}