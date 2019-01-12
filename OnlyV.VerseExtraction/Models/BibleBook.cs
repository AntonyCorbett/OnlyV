namespace OnlyV.VerseExtraction.Models
{
    internal class BibleBook
    {
        public string BookAbbreviatedName { get; set; }

        public string BookFullName { get; set; }

        public string FullPath { get; set; }

        public int BookNumber { get; set; }

        public bool HasSingleChapter =>
            BookNumber == 31 || BookNumber == 57 ||
            BookNumber == 63 || BookNumber == 64 ||
            BookNumber == 65;
    }
}
