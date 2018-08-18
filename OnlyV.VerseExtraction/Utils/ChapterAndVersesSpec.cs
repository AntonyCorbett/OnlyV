namespace OnlyV.VerseExtraction.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal class ChapterAndVersesSpec
    {
        private readonly List<ContiguousVerseSpec> _contiguousVerses = new List<ContiguousVerseSpec>();

        public IEnumerable<ContiguousVerseSpec> ContiguousVerses => _contiguousVerses;

        public void Add(int chapter, int startVerse, int endVerse)
        {
            if (endVerse < startVerse)
            {
                throw new Exception("End verse before start!");
            }

            _contiguousVerses.Add(new ContiguousVerseSpec
            {
                Chapter = chapter,
                StartVerse = startVerse,
                EndVerse = endVerse
            });

            ConsolidateChapter(chapter);

            _contiguousVerses.Sort((x, y) =>
            {
                var result = x.Chapter.CompareTo(y.Chapter);
                if (result == 0)
                {
                    result = x.StartVerse.CompareTo(y.StartVerse);
                }

                return result;
            });
        }

        public string ToTidyString()
        {
            var sb = new StringBuilder();
            var currentChapter = 0;
            foreach (var spec in _contiguousVerses)
            {
                if (spec.Chapter != currentChapter)
                {
                    if (currentChapter != 0)
                    {
                        sb.Append("; ");
                    }

                    sb.Append(spec.Chapter);
                    sb.Append(":");
                    currentChapter = spec.Chapter;
                }
                else
                {
                    if (currentChapter != 0)
                    {
                        sb.Append(",");
                    }
                }

                sb.Append(spec.StartVerse);
                if (spec.StartVerse != spec.EndVerse)
                {
                    sb.Append(spec.EndVerse == spec.StartVerse + 1 ? "," : "-");
                    sb.Append(spec.EndVerse);
                }
            }

            return sb.ToString();
        }

        public bool HasMultipleVerses()
        {
            return _contiguousVerses.Count > 1 ||
                   (_contiguousVerses.Count == 1 && _contiguousVerses[0].EndVerse != _contiguousVerses[0].StartVerse);
        }

        private void ConsolidateChapter(int chapter)
        {
            var chapterEntries = _contiguousVerses.Where(x => x.Chapter == chapter).ToArray();
            var numEntries = chapterEntries.Length;

            var consolidated = false;
            for (var n = 0; n < numEntries && !consolidated; ++n)
            {
                var entry1 = chapterEntries[n];
                for (var j = n + 1; j < numEntries && !consolidated; ++j)
                {
                    var entry2 = chapterEntries[j];
                    if (entry1.OverlapOrAdjoin(entry2))
                    {
                        // combine these 2...
                        _contiguousVerses.Remove(entry1);
                        _contiguousVerses.Remove(entry2);
                        _contiguousVerses.Add(new ContiguousVerseSpec
                        {
                            Chapter = chapter,
                            StartVerse = Math.Min(entry1.StartVerse, entry2.StartVerse),
                            EndVerse = Math.Max(entry1.EndVerse, entry2.EndVerse)
                        });

                        // and recurse...
                        ConsolidateChapter(chapter);
                        consolidated = true;
                    }
                }
            }
        }

        internal class ContiguousVerseSpec
        {
            public int Chapter { get; set; }

            public int StartVerse { get; set; }

            public int EndVerse { get; set; }

            public bool OverlapOrAdjoin(ContiguousVerseSpec rhs)
            {
                return Chapter == rhs.Chapter &&
                       StartVerse <= (rhs.EndVerse - 1) &&
                       (EndVerse + 1) >= rhs.StartVerse;
            }
        }
    }
}
