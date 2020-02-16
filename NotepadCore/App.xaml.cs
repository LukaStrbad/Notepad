using System.Windows;

namespace NotepadCore
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                var userSettings = Settings.Create();

                userSettings.AddFiles(e.Args);
                userSettings.SelectedFileIndex = userSettings.FilePaths.Length - 1;
                userSettings.Save();
            }
        }
    }
}