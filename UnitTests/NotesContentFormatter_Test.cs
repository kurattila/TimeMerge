using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeMerge.Utils;

namespace UnitTests
{
    [TestClass]
    public class NotesContentFormatter_Test
    {
        [TestMethod]
        public void NotesContentFormatter_EmptyNotes_Give_EmptyResult()
        {
            string result = NotesContentFormatter.NotesWithIndent(5, null);

            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void NotesContentFormatter_WhiteSpaceOnlyNotes_Give_EmptyResult()
        {
            string result = NotesContentFormatter.NotesWithIndent(5, "          \t   \t   \t  \n\r  \r\n");

            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void NotesContentFormatter_SingleLineNotes_GiveSingleLineResult()
        {
            string result = NotesContentFormatter.NotesWithIndent(0, "single line");

            Assert.AreEqual(NotesContentFormatter.Prefix + "single line", result);
        }

        [TestMethod]
        public void NotesContentFormatter_MultiLineNotes_GiveMultiLineResult()
        {
            string result = NotesContentFormatter.NotesWithIndent(0, "line1\r\n   line2\r\nline3");

            string expected = NotesContentFormatter.Prefix + "line1\r\n\t\t   line2\r\n\t\tline3";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NotesContentFormatter_SingleLineNote_WithIndentation()
        {
            string result = NotesContentFormatter.NotesWithIndent(2, "single line");

            Assert.AreEqual("\t\t" + NotesContentFormatter.Prefix + "single line", result);
        }

        [TestMethod]
        public void NotesContentFormatter_MultiLineNote_WithIndentation()
        {
            string result = NotesContentFormatter.NotesWithIndent(2, "line1\r\n   line2\r\nline3");

            string expected = "\t\t" + NotesContentFormatter.Prefix + "line1\r\n";
            expected += "\t\t\t\t   line2\r\n";
            expected += "\t\t\t\tline3";
            Assert.AreEqual(expected, result);
        }
    }
}
