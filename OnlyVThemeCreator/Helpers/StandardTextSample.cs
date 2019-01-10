namespace OnlyVThemeCreator.Helpers
{
    using System.Collections.Generic;

    internal class StandardTextSample
    {
        public StandardTextSample(int id, int bookNumber, string chapterAndVerses)
        {
            Id = id;
            BookNumber = bookNumber;
            ChapterAndVerses = chapterAndVerses;
        }

        public int Id { get; set; }

        public int BookNumber { get; set; }

        public string ChapterAndVerses { get; set; }

        public static IReadOnlyCollection<StandardTextSample> GetStandardList()
        {
            return new List<StandardTextSample>
            {
                new StandardTextSample(100, 1, "3:15,16"),
                new StandardTextSample(101, 19, "119:1-8"),
                new StandardTextSample(102, 66, "21:1-4"),
                new StandardTextSample(103, 43, "11:35")
            };
        }
    }
}
