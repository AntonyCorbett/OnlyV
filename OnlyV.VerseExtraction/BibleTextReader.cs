namespace OnlyV.VerseExtraction
{
    using Interfaces;
    using Models;
    using Serilog;

    public class BibleTextReader : IVerseReader, IBookLister
    {
        private readonly string _epubPath;

        public BibleTextReader(string epubPath)
        {
            _epubPath = epubPath;
        }

        public string ExtractVerseText(int bibleBook, string chapterAndVerse)
        {
            Log.Logger.Information("Extracting verse for book {bibleBook}, {chapterAndVerse}", bibleBook, chapterAndVerse);
            throw new System.NotImplementedException();
        }

        public BibleBookData ExtractBookData()
        {
            Log.Logger.Information("Extracting book data");
            throw new System.NotImplementedException();
        }
    }
}
