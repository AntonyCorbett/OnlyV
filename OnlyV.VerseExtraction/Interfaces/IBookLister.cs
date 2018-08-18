namespace OnlyV.VerseExtraction.Interfaces
{
    using System.Collections.Generic;
    using Models;

    public interface IBookLister
    {
        IReadOnlyCollection<BibleBookData> ExtractBookData();
    }
}
