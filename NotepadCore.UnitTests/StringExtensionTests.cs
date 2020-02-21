using System.Linq;
using NotepadCore.ExtensionMethods;
using NUnit.Framework;

namespace NotepadCore.UnitTests
{
    [TestFixture]
    public class StringExtensionTests
    {
        [Test]
        public static void IndexesOf_ReturnsCorrectValues()
        {
            string testString = "test_test_test";
            int[] expectedResult = {0, 5, 10};

            var indexes = testString.IndexesOf("test");
            
            Assert.That(expectedResult.SequenceEqual(indexes));
        }
    }
}