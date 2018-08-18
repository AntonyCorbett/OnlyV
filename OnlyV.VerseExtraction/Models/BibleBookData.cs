namespace OnlyV.VerseExtraction.Models
{
    using System.Collections.Generic;

    public class BibleBookData
    {
        public string Name { get; set; }

        public int Number { get; set; }

        public Dictionary<int, VerseRange> ChapterAndVerseCount { get; } = new Dictionary<int, VerseRange>();

        public int ChapterCount => ChapterAndVerseCount.Count;

        public void AddChapter(int chapter, VerseRange verseRange)
        {
            ChapterAndVerseCount.Add(chapter, verseRange);
        }

        public VerseRange GetVerseRange(int chapter)
        {
            return ChapterAndVerseCount.TryGetValue(chapter, out var range) ? range : null;
        }
    }
}
