using System;
using System.Threading;
using Microsoft.VisualBasic.FileIO;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using NUnit.Framework;

namespace NotepadCore.UnitTests
{
    [TestFixture]
    public class TextEditorTests
    {
        [Test]
        public void WriteLineNumbers_Called_TextEditorsHaveSameAmountOfLines()
        {
            var textEditor = new TextEditor("testFile.txt");
            Assert.That(textEditor.LineTextBox.Document.Blocks.Count == textEditor.MainTextBox.Document.Blocks.Count);
        }
    }
}