using OnlyV.ImageCreation;

namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OnlyV.VerseExtraction.Models;
    using OnlyV.VerseExtraction.Parser;

    [TestClass]
    public class IntegrationTests
    {
        private const string EpubPath = "nwt_E.epub";

        [TestMethod]
        public void TestBibleEpubParser()
        {
            var parser = new BibleEpubParser(EpubPath);
            var bookData = parser.ExtractBookData();
            Assert.IsNotNull(bookData);

            // Gen 1:1
            var formattingOptions = new FormattingOptions();
            var s = parser.ExtractVerseText(1, 1, 1, formattingOptions);
            Assert.IsNotNull(s);

            var s2 = parser.ExtractVersesText(1, "1:1-3", formattingOptions);
            Assert.IsNotNull(s2);
        }

        [TestMethod]
        public void TestImageCreation()
        {
            BibleTextImage image = new BibleTextImage();
            var images = image.Generate(EpubPath, 1, "3:15");

        }
    }
}
