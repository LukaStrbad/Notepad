using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Documents;
using NUnit.Framework;

namespace NotepadCore.UnitTests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class TextEditorTests
    {
        private const string DocumentPath = "testFile.txt";
        [Test]
        public void WriteLineNumbers_Called_TextBoxesHaveSameBlockCount()
        {
            var textEditor = new TextEditor(DocumentPath);
            Assert.That(textEditor.LineTextBox.Document.Blocks.Count == textEditor.MainTextBox.Document.Blocks.Count);
        }

        [Test]
        public void HighlightCurrentLine_Called_RetainsText()
        {
            var textEditor = new TextEditor(DocumentPath);
            textEditor.MainTextBox.CaretPosition = textEditor.MainTextBox.Document.ContentStart.GetPositionAtOffset(0);

            textEditor.MainTextBox.CaretPosition.Paragraph.Inlines.Clear();
            const string text = "    static void Main(string[] args) {";
            textEditor.MainTextBox.CaretPosition.Paragraph.Inlines.Add(new Run(text));

            var textRange = new TextRange(textEditor.MainTextBox.CaretPosition.Paragraph.ContentStart,
                textEditor.MainTextBox.CaretPosition.Paragraph.ContentEnd);

            Assert.That(textRange.Text == text);
        }

        [Test]
        public void TabSize_ValueLessThanZeroOrZero_ThrowsArgumentException( [Values(int.MinValue, -100, -1)] int tabSize)
        {
            var textEditor = new TextEditor();
            Assert.Throws<ArgumentException>(() => textEditor.TabSize = tabSize);
        }

        [Test]
        public void TabSize_ValueMoreThanZero_DoesntThrowException([Values(1, 100, int.MaxValue)] int tabSize)
        {
            var textEditor = new TextEditor();
            Assert.DoesNotThrow((() => textEditor.TabSize = tabSize));
        }

        [Test]
        public void Text_Get_HasCorrectText()
        {
            var textEditor = new TextEditor(DocumentPath);
            string text = File.ReadAllText(DocumentPath);
            Assert.That(textEditor.Text == text);
        }
    }
}