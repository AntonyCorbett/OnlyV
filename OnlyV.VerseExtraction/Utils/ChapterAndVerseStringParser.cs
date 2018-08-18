namespace OnlyV.VerseExtraction.Utils
{
    using System;

    internal static class ChapterAndVerseStringParser
    {
        public static ChapterAndVersesSpec Parse(string chapterAndVerse)
        {
            ChapterAndVersesSpec result = new ChapterAndVersesSpec();

            string[] tokens = chapterAndVerse.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 2)
            {
                if (int.TryParse(tokens[0], out var chapter))
                {
                    var verseTokens = tokens[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (verseTokens.Length > 0)
                    {
                        result = new ChapterAndVersesSpec();

                        foreach (var t in verseTokens)
                        {
                            var rangeTokens = t.Split(new[] { '-', '—' }, StringSplitOptions.RemoveEmptyEntries);
                            if (rangeTokens.Length == 1 || rangeTokens.Length == 2)
                            {
                                if (int.TryParse(rangeTokens[0], out var startVerse))
                                {
                                    if (rangeTokens.Length == 2)
                                    {
                                        if (int.TryParse(rangeTokens[1], out var endVerse))
                                        {
                                            result.Add(chapter, startVerse, endVerse);
                                        }
                                    }
                                    else
                                    {
                                        result.Add(chapter, startVerse, startVerse);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
