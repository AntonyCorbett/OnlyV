using OnlyV.VerseExtraction.Cache;

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
        private static BibleBookDataCache Cache = new BibleBookDataCache();

        private readonly string _epubPath;
        private readonly BibleEpubParser _parser;

        public BibleTextReader(string epubPath)
        {
            _epubPath = epubPath;
            _parser = new BibleEpubParser(epubPath);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_parser", Justification = "False positive")]
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
            var result = Cache.Get(_epubPath);
            if (result == null)
            {
                result = ReadBookData();
                Cache.Add(_epubPath, result);
            }

            return result;
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
