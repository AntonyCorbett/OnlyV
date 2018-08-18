namespace OnlyV.VerseExtraction.Models
{
    internal class BookChapter
    {
        public BibleBook Book { get; set; }

        public int Chapter { get; set; }

        public string FullPath { get; set; }

        public VerseRange VerseRange { get; set; }
    }
}
