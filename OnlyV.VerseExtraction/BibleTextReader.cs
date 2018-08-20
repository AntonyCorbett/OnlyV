namespace OnlyV.VerseExtraction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces;
    using Models;
    using Parser;
    using Serilog;
    using Utils;

    public sealed class BibleTextReader : IVerseReader, IBookLister, IDisposable
    {
        private readonly BibleEpubParser _parser;
        private readonly Lazy<IReadOnlyCollection<BibleBookData>> _cachedBookData;

        public BibleTextReader(string epubPath)
        {
            _parser = new BibleEpubParser(epubPath);
            _cachedBookData = new Lazy<IReadOnlyCollection<BibleBookData>>(ReadBookData);
        }

        public void Dispose()
        {
            _parser?.Dispose();
        }

        public string ExtractVerseText(
            int bibleBook, 
            string chapterAndVerse,
            FormattingOptions formattingOptions)
        {
            Log.Logger.Information("Extracting verse for book {bibleBook}, {chapterAndVerse}", bibleBook, chapterAndVerse);
            return _parser.ExtractVersesText(bibleBook, chapterAndVerse, formattingOptions);
        }

        public IReadOnlyCollection<BibleBookData> ExtractBookData()
        {
            return _cachedBookData.Value;
        }

        public string GenerateVerseTitle(int bookNumber, string chapterAndVerses)
        {
            string bookName = GetBookName(bookNumber);
            var cv = ChapterAndVerseStringParser.Parse(chapterAndVerses);
            return string.Concat(bookName, " ", cv.ToTidyString());
        }

        private IReadOnlyCollection<BibleBookData> ReadBookData()
        {
            Log.Logger.Information("Reading book data");
            return _parser.ExtractBookData();
        }

        private string GetBookName(int bookNumber)
        {
            var book = ExtractBookData().FirstOrDefault(x => x.Number == bookNumber);
            return book?.Name;
        }
    }
}
