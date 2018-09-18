namespace OnlyV.VerseExtraction.Cache
{
    using System.Collections.Generic;
    using Models;

    internal class BibleBookDataCache
    {
        private readonly Dictionary<string, IReadOnlyCollection<BibleBookData>> _data = 
            new Dictionary<string, IReadOnlyCollection<BibleBookData>>();

        public IReadOnlyCollection<BibleBookData> Get(string epubPath)
        {
            _data.TryGetValue(epubPath, out var data);
            return data;
        }

        public void Add(string epubPath, IReadOnlyCollection<BibleBookData> data)
        {
            _data.Add(epubPath, data);
        }
    }
}
