using System;
using System.IO;
using System.Xml.Linq;
using OnlyV.VerseExtraction.Utils;

namespace OnlyV.VerseExtraction.Cache
{
    internal class XDocumentCache
    {
        private const int MaxEntries = 16;
        private readonly MostRecentlyUsedList<string, XDocument> _data = new MostRecentlyUsedList<string, XDocument>(MaxEntries);
        
        public XDocument Get(string epubPath, DateTime epubCreationStamp, string docKey)
        {
            _data.TryGetValue(GenerateKey(epubPath, epubCreationStamp, docKey), out var data);
            return data;
        }

        public void Add(string epubPath, DateTime epubCreationStamp, string docKey, XDocument data)
        {
            if (data != null)
            {
                _data.Add(GenerateKey(epubPath, epubCreationStamp, docKey), data);
            }
        }

        private static string GenerateKey(string epubPath, DateTime creationStamp, string docKey)
        {
            return $"{docKey}-{creationStamp.Ticks}-{Path.GetFileName(epubPath)}";
        }
    }
}
