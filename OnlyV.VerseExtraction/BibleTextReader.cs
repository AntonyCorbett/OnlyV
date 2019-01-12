namespace OnlyV.VerseExtraction
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Cache;
    using Interfaces;
    using Models;
    using Parser;
    using Serilog;
    using Utils;

    public sealed class BibleTextReader : IVerseReader, IBookLister, IDisposable
    {
        private static readonly BibleBookDataCache Cache = new BibleBookDataCache();

        private readonly string _epubPath;
        private readonly DateTime _epubCreationStampUtc;
        private readonly BibleEpubParser _parser;

        public BibleTextReader(string epubPath)
        {
            _epubPath = epubPath;

            var fi = new FileInfo(_epubPath);
            _epubCreationStampUtc = fi.CreationTimeUtc;

            _parser = new BibleEpubParser(epubPath);
            _parser.VerseFetchEvent += HandleVerseFetchEvent;
        }

        public event EventHandler<VerseAndText> VerseFetchEvent;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_parser", Justification = "False positive")]
        public void Dispose()
        {
            _parser?.Dispose();
        }

        public string ExtractVerseText(
            int bibleBook, 
            string chapterAndVerses,
            FormattingOptions formattingOptions)
        {
            Log.Logger.Information("Extracting verse for book {bibleBook}, {chapterAndVerse}", bibleBook, chapterAndVerses);
            return _parser.ExtractVersesText(bibleBook, chapterAndVerses, formattingOptions);
        }

        public IReadOnlyCollection<VerseAndText> ExtractVerseTextArray(
            int bibleBook,
            string chapterAndVerses,
            FormattingOptions formattingOptions)
        {
            Log.Logger.Information("Extracting verse array for book {bibleBook}, {chapterAndVerse}", bibleBook, chapterAndVerses);
            return _parser.ExtractVersesTextArray(bibleBook, chapterAndVerses, formattingOptions);
        }

        public IReadOnlyCollection<BibleBookData> ExtractBookData()
        {
            var result = Cache.Get(_epubPath, _epubCreationStampUtc);
            if (result == null)
            {
                result = ReadBookData();
                Cache.Add(_epubPath, _epubCreationStampUtc, result);
            }

            return result;
        }

        public string GenerateVerseTitle(
            int bookNumber, 
            string chapterAndVerses, 
            bool spaceBetweenVerseNumbers,
            bool useAbbreviatedBookNames)
        {
            string bookName = GetBookNameForImage(bookNumber, useAbbreviatedBookNames);
            var cv = ChapterAndVerseStringParser.Parse(chapterAndVerses);
            return string.Concat(bookName, " ", cv.ToTidyString(spaceBetweenVerseNumbers));
        }

        private IReadOnlyCollection<BibleBookData> ReadBookData()
        {
            Log.Logger.Information("Reading book data");
            return _parser.ExtractBookData();
        }

        private string GetBookNameForImage(int bookNumber, bool useAbbreviatedBookName)
        {
            var book = ExtractBookData().FirstOrDefault(x => x.Number == bookNumber);
            return useAbbreviatedBookName ? book?.AbbreviatedName : book?.FullName;
        }

        private void HandleVerseFetchEvent(object sender, VerseAndText e)
        {
            VerseFetchEvent?.Invoke(this, e);
        }
    }
}
