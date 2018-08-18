namespace OnlyV.VerseExtraction.Interfaces
{
    using Models;

    public interface IBookLister
    {
        BibleBookData ExtractBookData();
    }
}
