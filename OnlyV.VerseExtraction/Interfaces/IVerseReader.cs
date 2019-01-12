namespace OnlyV.VerseExtraction.Interfaces
{
    using System.Collections.Generic;
    using Models;

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
