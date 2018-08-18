namespace OnlyV.VerseExtraction.Interfaces
{
    using Models;

    public interface IVerseReader
    {
        string ExtractVerseText(
            int bibleBook, 
            string chapterAndVerse,
            FormattingOptions formattingOptions);
    }
}
