namespace OnlyV.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using OnlyV.VerseExtraction;
    using OnlyV.VerseExtraction.Models;

    internal class BibleVersesService : IBibleVersesService
    {
        private string _epubPath;
        private BibleTextReader _reader;

        public string EpubPath
        {
            get => _epubPath;
            set
            {
                if (value != EpubPath)
                {
                    _epubPath = value;
                    InitReader();
                }
            }
        }

        public IReadOnlyCollection<BibleBookData> GetBookData()
        {
            CheckReaderAvailable();
            return _reader.ExtractBookData();
        }

        private void CheckReaderAvailable()
        {
            if (_reader == null)
            {
                throw new Exception(Properties.Resources.READER_NOT_INIT);
            }
        }

        private void InitReader()
        {
            CheckEpubAvailable();
            _reader = new BibleTextReader(_epubPath);
        }

        private void CheckEpubAvailable()
        {
            if (string.IsNullOrEmpty(_epubPath))
            {
                throw new Exception(Properties.Resources.EPUB_NOT_SPECIFIED);
            }

            if (!File.Exists(_epubPath))
            {
                throw new Exception(Properties.Resources.EPUB_NOT_FOUND);
            }
        }
    }
}
