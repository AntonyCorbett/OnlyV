namespace OnlyV.Services.Bible
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using OnlyV.Services.Options;
    using OnlyV.VerseExtraction;
    using OnlyV.VerseExtraction.Models;

    internal class BibleVersesService : IBibleVersesService
    {
        private readonly IOptionsService _optionsService;
        private string _epubPath;
        private BibleTextReader _reader;
        private Lazy<IReadOnlyCollection<BibleBookData>> _bookData;

        public BibleVersesService(IOptionsService optionsService)
        {
            _optionsService = optionsService;
            _optionsService.Options.EpubPathChangedEvent += HandleEpubPathChangedEvent;

            EpubPath = _optionsService.Options.EpubPath;
        }

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
            return _bookData.Value;
        }

        public int GetChapterCount(int bookNumber)
        {
            var book = _bookData.Value.Single(x => x.Number == bookNumber);
            return book.ChapterCount;
        }

        public VerseRange GetVerseRange(int bookNumber, int chapterNumber)
        {
            var book = _bookData.Value.Single(x => x.Number == bookNumber);
            return book.GetVerseRange(chapterNumber);
        }

        private void InitReader()
        {
            CheckEpubAvailable();
            _reader = new BibleTextReader(_epubPath);
            _bookData = new Lazy<IReadOnlyCollection<BibleBookData>>(ReadBookData);
        }

        private IReadOnlyCollection<BibleBookData> ReadBookData()
        {
            return _reader?.ExtractBookData();
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

        private void HandleEpubPathChangedEvent(object sender, System.EventArgs e)
        {
            EpubPath = _optionsService.Options.EpubPath;
        }
    }
}
