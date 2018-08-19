namespace OnlyV.Services
{
    using System.Collections.Generic;
    using OnlyV.VerseExtraction.Models;

    internal interface IBibleVersesService
    {
        string EpubPath { get; set; }

        IReadOnlyCollection<BibleBookData> GetBookData();
    }
}
