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
            if (e.Args.Length > 0) // Provjera ima li argumenata
            {
                // Stvaranje instance korisničkih postavki
                var userSettings = UserSettings.Create();

                userSettings.AddFiles(e.Args); // Dodavanje datoteka u postavke

                // Postavljanje indeksa otvorene datoteke kako bi se pri 
                // pokretanju aplikacije otvorila zadnja dodana datoteka
                userSettings.SelectedFileIndex = userSettings.Editors.Length - 1;
                userSettings.Save(); // Spremanje postavki
            }
        }
    }
}
