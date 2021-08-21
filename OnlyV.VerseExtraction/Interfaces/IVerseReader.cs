using System.Collections.Generic;
using OnlyV.VerseExtraction.Models;

namespace OnlyV.VerseExtraction.Interfaces
{
    public interface IVerseReader
    {
        string ExtractVerseText(
            int bibleBook, 
            string chapterAndVerses,
            FormattingOptions formattingOptions);

        IReadOnlyCollection<VerseAndText> ExtractVerseTextArray(
            int bibleBook,
            string chapterAndVerses,
            FormattingOptions formattingOptions);

        string GenerateVerseTitle(
            int bookNumber, 
            string chapterAndVerses, 
            bool spaceBetweenVerseNumbers,
            bool useAbbreviatedBookName);
    }
}
