namespace OnlyVThemeCreator.ViewModel
{
    public class VerseTextItem
    {
        public VerseTextItem(int id, string name, int bookNumber, string chapterAndVerses)
        {
            Id = id;
            Name = name;
            BookNumber = bookNumber;
            ChapterAndVerses = chapterAndVerses;
        }

        public int Id { get; }

        public string Name { get; }

        public int BookNumber { get; }

        public string ChapterAndVerses { get; }
    }
}
