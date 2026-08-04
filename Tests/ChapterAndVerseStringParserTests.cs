using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnlyV.VerseExtraction.Utils;

namespace Tests
{
    [TestClass]
    public class ChapterAndVerseStringParserTests
    {
        [TestMethod]
        public void Parse_SingleVerse_ReturnsSingleContiguousVerse()
        {
            var result = ChapterAndVerseStringParser.Parse("3:15");

            var verse = result.ContiguousVerses.Single();
            Assert.AreEqual(3, verse.Chapter);
            Assert.AreEqual(15, verse.StartVerse);
            Assert.AreEqual(15, verse.EndVerse);
        }

        [TestMethod]
        public void Parse_HyphenRange_ReturnsCorrectRange()
        {
            var result = ChapterAndVerseStringParser.Parse("3:15-17");

            var verse = result.ContiguousVerses.Single();
            Assert.AreEqual(3, verse.Chapter);
            Assert.AreEqual(15, verse.StartVerse);
            Assert.AreEqual(17, verse.EndVerse);
        }

        [TestMethod]
        public void Parse_EmDashRange_ParsesSameAsHyphen()
        {
            var result = ChapterAndVerseStringParser.Parse("3:15—17");

            var verse = result.ContiguousVerses.Single();
            Assert.AreEqual(15, verse.StartVerse);
            Assert.AreEqual(17, verse.EndVerse);
        }

        [TestMethod]
        public void Parse_NonAdjoiningCommaSeparatedVerses_ReturnsSeparateEntries()
        {
            var result = ChapterAndVerseStringParser.Parse("3:15,17");

            var verses = result.ContiguousVerses.OrderBy(v => v.StartVerse).ToArray();

            Assert.AreEqual(2, verses.Length);
            Assert.AreEqual(15, verses[0].StartVerse);
            Assert.AreEqual(15, verses[0].EndVerse);
            Assert.AreEqual(17, verses[1].StartVerse);
            Assert.AreEqual(17, verses[1].EndVerse);
        }

        [TestMethod]
        public void Parse_MissingColon_ReturnsEmptySpec()
        {
            var result = ChapterAndVerseStringParser.Parse("abc");

            Assert.AreEqual(0, result.ContiguousVerses.Count());
        }

        [TestMethod]
        public void Parse_NonNumericChapter_ReturnsEmptySpec()
        {
            var result = ChapterAndVerseStringParser.Parse("abc:15");

            Assert.AreEqual(0, result.ContiguousVerses.Count());
        }

        [TestMethod]
        public void Parse_NoVersesAfterColon_ReturnsEmptySpec()
        {
            var result = ChapterAndVerseStringParser.Parse("3:");

            Assert.AreEqual(0, result.ContiguousVerses.Count());
        }
    }
}
