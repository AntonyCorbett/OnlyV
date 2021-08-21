using System.Collections.Generic;
using OnlyV.VerseExtraction.Models;

namespace OnlyV.VerseExtraction.Interfaces
{
    public interface IBookLister
    {
        IReadOnlyCollection<BibleBookData> ExtractBookData();
    }
}
