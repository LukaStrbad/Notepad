using System;
using System.Linq;
using System.Reflection;
using NotepadCore.Settings;
using NotepadCore.SyntaxHighlighters;
using NUnit.Framework;

namespace NotepadCore.UnitTests
{
    [TestFixture]
    public class SettingsTests
    {
        [Test]
        public static void Create_Call_DoesntThrowExceptions()
        {
            Assert.DoesNotThrow(() => UserSettings.Create());
        }

        [Test]
        public static void Save_SavesCorrectValues()
        {
            var userSettings = UserSettings.Create();
            userSettings.Editors = new[] {new EditorInfo(HighlightingLanguage.None, "path.txt")};
            userSettings.TabSize = 4;
            userSettings.UseSpaces = true;
            userSettings.EditorFontFamily = "Consolas";
            userSettings.EditorFontSize = 12;
            userSettings.SelectedFileIndex = 0;
            userSettings.ShowLineNumbers = true;
            userSettings.Save();

            var loadedSettings = UserSettings.Create();

            foreach (var propertyInfo in typeof(UserSettings).GetProperties(BindingFlags.Public))
            {
                Assert.That(propertyInfo.GetValue(userSettings) == propertyInfo.GetValue(loadedSettings));
            }
        }

        [Test]
        public static void AddFiles_AddsCorrectValues()
        {
            var userSettings = UserSettings.Create();
            userSettings.Editors = null;
            
            var paths = new[] {"test.txt", "test.txt", "test2.txt"};
            
            userSettings.AddFiles(paths);

            var savedPaths = userSettings.Editors.Select(x => x.FilePath);
            Assert.That(savedPaths.Contains("test.txt") && savedPaths.Contains("test2.txt"));
        }
    }
}