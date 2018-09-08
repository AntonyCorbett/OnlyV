namespace OnlyV.Services.Bible
{
    using System.Collections.Generic;
    using OnlyV.VerseExtraction.Models;

    internal interface IBibleVersesService
    {
        string EpubPath { get; set; }

        IReadOnlyCollection<BibleBookData> GetBookData();

        int GetChapterCount(int bookNumber);

        VerseRange GetVerseRange(int bookNumber, int chapterNumber);

        bool IsValidBibleEpub(string epubPath);
    }
}
