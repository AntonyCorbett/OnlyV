namespace OnlyV.VerseExtraction
{
    using System.Collections.Generic;
    using Interfaces;
    using Models;
    using Parser;
    using Serilog;

    public class BibleTextReader : IVerseReader, IBookLister
    {
        private readonly BibleEpubParser _parser;

        public BibleTextReader(string epubPath)
        {
            _parser = new BibleEpubParser(epubPath);
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
            Log.Logger.Information("Extracting book data");
            return _parser.ExtractBookData();
        }
    }
}
