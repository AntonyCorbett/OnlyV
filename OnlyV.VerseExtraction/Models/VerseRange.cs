namespace OnlyV.VerseExtraction.Models
{
    public class VerseRange
    {
        public int FirstVerse { get; set; }

        public int LastVerse { get; set; }

        public bool IsValid()
        {
            return FirstVerse > 0 && LastVerse > 0 && LastVerse > FirstVerse;
        }
    }
}
