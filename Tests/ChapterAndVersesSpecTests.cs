using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnlyV.VerseExtraction.Utils;

namespace Tests
{
    [TestClass]
    public class ChapterAndVersesSpecTests
    {
        [TestMethod]
        public void Add_EndBeforeStart_Throws()
        {
            var spec = new ChapterAndVersesSpec();

            Assert.ThrowsException<Exception>(() => spec.Add(1, 10, 5));
        }

        [TestMethod]
        public void Add_AdjoiningRanges_ConsolidatesIntoOne()
        {
            var spec = new ChapterAndVersesSpec();

            spec.Add(1, 1, 3);
            spec.Add(1, 4, 6);

            var verse = spec.ContiguousVerses.Single();
            Assert.AreEqual(1, verse.StartVerse);
            Assert.AreEqual(6, verse.EndVerse);
        }

        [TestMethod]
        public void Add_NonAdjoiningRanges_StaySeparate()
        {
            var spec = new ChapterAndVersesSpec();

            spec.Add(1, 1, 3);
            spec.Add(1, 10, 12);

            Assert.AreEqual(2, spec.ContiguousVerses.Count());
        }

        [TestMethod]
        public void HasMultipleVerses_SingleVerse_ReturnsFalse()
        {
            var spec = new ChapterAndVersesSpec();
            spec.Add(1, 5, 5);

            Assert.IsFalse(spec.HasMultipleVerses());
        }

        [TestMethod]
        public void HasMultipleVerses_Range_ReturnsTrue()
        {
            var spec = new ChapterAndVersesSpec();
            spec.Add(1, 5, 6);

            Assert.IsTrue(spec.HasMultipleVerses());
        }

        [TestMethod]
        public void ToTidyString_SingleChapterBook_OmitsChapterPrefix()
        {
            var spec = new ChapterAndVersesSpec();
            spec.Add(1, 3, 3);
            spec.Add(1, 5, 5);

            var result = spec.ToTidyString(bookHasSingleChapter: true, spaceBetweenVerseNumbers: false);

            Assert.AreEqual("3,5", result);
        }

        [TestMethod]
        public void ToTidyString_MultiChapter_UsesSemicolonSeparatorAndChapterPrefix()
        {
            var spec = new ChapterAndVersesSpec();
            spec.Add(1, 3, 3);
            spec.Add(2, 5, 5);

            var result = spec.ToTidyString(bookHasSingleChapter: false, spaceBetweenVerseNumbers: false);

            Assert.AreEqual("1:3; 2:5", result);
        }

        [TestMethod]
        public void ToTidyString_MultiChapterOnSingleChapterBook_Throws()
        {
            var spec = new ChapterAndVersesSpec();
            spec.Add(1, 3, 3);
            spec.Add(2, 5, 5);

            Assert.ThrowsException<ArgumentException>(
                () => spec.ToTidyString(bookHasSingleChapter: true, spaceBetweenVerseNumbers: false));
        }
    }
}
