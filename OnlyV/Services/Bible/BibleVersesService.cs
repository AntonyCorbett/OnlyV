namespace OnlyV.Services.Bible
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Options;
    using Serilog;
    using VerseExtraction;
    using VerseExtraction.Models;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class BibleVersesService : IBibleVersesService, IDisposable
    {
        private readonly IOptionsService _optionsService;
        private string _epubPath;
        private BibleTextReader _reader;
        
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

        public void Dispose()
        {
            _reader?.Dispose();
        }

        public bool IsValidBibleEpub(string epubPath)
        {
            try
            {
                if (!File.Exists(epubPath))
                {
                    return false;
                }

                using (var reader = new BibleTextReader(epubPath))
                {
                    var bookData = reader.ExtractBookData();
                    return bookData != null && bookData.Any();
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($@"Could not read epub {epubPath}", ex);
                return false;
            }
        }

        public IReadOnlyCollection<BibleBookData> GetBookData()
        {
            return _reader.ExtractBookData();
        }

        public int GetChapterCount(int bookNumber)
        {
            var book = _reader.ExtractBookData().Single(x => x.Number == bookNumber);
            return book.ChapterCount;
        }

        public VerseRange GetVerseRange(int bookNumber, int chapterNumber)
        {
            var book = _reader.ExtractBookData().Single(x => x.Number == bookNumber);
            return book.GetVerseRange(chapterNumber);
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

        private void HandleEpubPathChangedEvent(object sender, System.EventArgs e)
        {
            EpubPath = _optionsService.Options.EpubPath;
        }
    }
}
