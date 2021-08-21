using System.Collections.Generic;
using OnlyV.VerseExtraction.Models;

namespace OnlyV.Services.Bible
{
    internal interface IBibleVersesService
    {
        string EpubPath { get; set; }

        IReadOnlyCollection<BibleBookData> GetBookData();

        int GetChapterCount(int bookNumber);

        VerseRange GetVerseRange(int bookNumber, int chapterNumber);

        bool IsValidBibleEpub(string epubPath);

        void CloseReader();
    }
}
