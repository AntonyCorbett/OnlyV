namespace OnlyV.VerseExtraction.Interfaces
{
    public interface IVerseReader
    {
        string ExtractVerseText(int bibleBook, string chapterAndVerse);
    }
}
