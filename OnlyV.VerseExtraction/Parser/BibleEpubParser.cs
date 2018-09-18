namespace OnlyV.VerseExtraction.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using Serilog;
    using Utils;

    internal sealed class BibleEpubParser : IDisposable
    {
        private readonly EpubAsArchive _epub;
        private readonly Lazy<IReadOnlyList<BibleBook>> _bibleBooks;
        private readonly Lazy<IReadOnlyList<BookChapter>> _bookChapters;

        public BibleEpubParser(string epubPath)
        {
            _epub = new EpubAsArchive(epubPath);
            
            _bibleBooks = new Lazy<IReadOnlyList<BibleBook>>(GenerateBibleBooksList);
            _bookChapters = new Lazy<IReadOnlyList<BookChapter>>(GenerateChapterList);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_epub", Justification = "False positive")]
        public void Dispose()
        {
            _epub?.Dispose();
        }

        public IReadOnlyCollection<BibleBookData> ExtractBookData()
        {
            var result = new List<BibleBookData>();

            foreach (var book in _bibleBooks.Value)
            {
                var rec = new BibleBookData
                {
                    Name = book.BookName,
                    Number = book.BookNumber
                };

                var chapters = _bookChapters.Value.Where(x => x.Book.Equals(book));
                foreach (var chapter in chapters)
                {
                    rec.AddChapter(chapter.Chapter, chapter.VerseRange);
                }

                result.Add(rec);
            }

            return result;
        }

        public string ExtractVersesText(
            int bibleBook,
            string chapterAndVerses,
            FormattingOptions formattingOptions)
        {
            Log.Logger.Information("Extracting Bible verse");

            var verses = ChapterAndVerseStringParser.Parse(chapterAndVerses);
            return _epub.GetBibleTexts(_bookChapters.Value, bibleBook, verses, formattingOptions);
        }

        public string ExtractVerseText(
            int bibleBook, 
            int chapter, 
            int verse, 
            FormattingOptions formattingOptions)
        {
            Log.Logger.Information("Extracting Bible verse");
            return _epub.GetBibleText(_bookChapters.Value, bibleBook, chapter, verse, formattingOptions);
        }

        private IReadOnlyList<BookChapter> GenerateChapterList()
        {
            Log.Logger.Information("Generating chapter list");
            return _epub.GenerateBibleChaptersList(_bibleBooks.Value);
        }

        private IReadOnlyList<BibleBook> GenerateBibleBooksList()
        {
            Log.Logger.Information("Initialising books");
            return _epub.GenerateBibleBooksList();
        }
    }
}
