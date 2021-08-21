using System;
using System.Collections.Generic;
using System.IO;
using OnlyV.VerseExtraction.Models;

namespace OnlyV.VerseExtraction.Cache
{
    internal class BibleBooksListCache
    {
        private readonly Dictionary<string, IReadOnlyList<BibleBook>> _data =
            new Dictionary<string, IReadOnlyList<BibleBook>>();

        public IReadOnlyList<BibleBook> Get(string epubPath, DateTime epubCreationStamp)
        {
            _data.TryGetValue(GenerateKey(epubPath, epubCreationStamp), out var data);
            return data;
        }

        public void Add(
            string epubPath,
            DateTime epubCreationStamp,
            IReadOnlyList<BibleBook> data)
        {
            if (data != null)
            {
                _data.Add(GenerateKey(epubPath, epubCreationStamp), data);
            }
        }

        private static string GenerateKey(string epubPath, DateTime creationStamp)
        {
            return $"{creationStamp.Ticks}-{Path.GetFileName(epubPath)}";
        }
    }
}
