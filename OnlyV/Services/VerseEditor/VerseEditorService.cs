namespace OnlyV.Services.VerseEditor
{
    using System.Collections.Generic;
    using System.Linq;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class VerseEditorService : IVerseEditorService
    {
        private readonly List<VerseEditorEntry> _data = new List<VerseEditorEntry>();

        public void Add(string epubPath, int bookNumber, int chapter, int verse, string modifiedText)
        {
            var existing = _data.SingleOrDefault(x => x.EpubPath == epubPath &&
                                                      x.BookNumber == bookNumber &&
                                                      x.Chapter == chapter &&
                                                      x.Verse == verse);
            if (existing != null)
            {
                existing.ModifiedVerseText = modifiedText;
            }
            else
            {
                _data.Add(new VerseEditorEntry
                {
                    EpubPath = epubPath,
                    BookNumber = bookNumber,
                    Chapter = chapter,
                    Verse = verse,
                    ModifiedVerseText = modifiedText
                });    
            }
        }

        public void Delete(string epubPath, int bookNumber, int chapter, int verse)
        {
            _data.RemoveAll(x => x.EpubPath == epubPath &&
                                 x.BookNumber == bookNumber &&
                                 x.Chapter == chapter &&
                                 x.Verse == verse);
        }

        public string Get(string epubPath, int bookNumber, int chapter, int verse)
        {
            var existing = _data.SingleOrDefault(x => x.EpubPath == epubPath &&
                                                      x.BookNumber == bookNumber &&
                                                      x.Chapter == chapter &&
                                                      x.Verse == verse);

            return existing?.ModifiedVerseText;
        }
    }
}
