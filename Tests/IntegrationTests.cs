namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OnlyV.VerseExtraction.Parser;

    [TestClass]
    public class IntegrationTests
    {
        [TestMethod]
        public void TestBibleEpubParser()
        {
            var parser = new BibleEpubParser("nwt_E.epub");
            var bookData = parser.ExtractBookData();

        }
    }
}
