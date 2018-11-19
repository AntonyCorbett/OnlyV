namespace OnlyV.ViewModel
{
    using System.Collections.ObjectModel;
    using GalaSoft.MvvmLight;
    using OnlyV.Helpers;
    using OnlyV.Services.Options;
    using OnlyV.Services.VerseEditor;
    using OnlyV.VerseExtraction;
    using OnlyV.VerseExtraction.Models;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class EditTextViewModel : ViewModelBase
    {
        private readonly IOptionsService _optionsService;
        private readonly IVerseEditorService _verseEditorService;

        public EditTextViewModel(IOptionsService optionsService, IVerseEditorService verseEditorService)
        {
            _optionsService = optionsService;
            _verseEditorService = verseEditorService;

            AddDesignTimeItems();
        }

        public ObservableCollection<EditVerseTextViewModel> Verses { get; } = new ObservableCollection<EditVerseTextViewModel>();

        public void UpdateVerseEditorService()
        {
            foreach (var vs in Verses)
            {
                if (!vs.IsModified)
                {
                    _verseEditorService.Delete(vs.EpubPath, vs.BookNumber, vs.Chapter, vs.Verse);
                }
                else
                {
                    _verseEditorService.Add(vs.EpubPath, vs.BookNumber, vs.Chapter, vs.Verse, vs.ModifiedVerseText);
                }
            }
        }

        public void Init(int bookNumber, string chapterAndVersesString)
        {
            Verses.Clear();

            using (var reader = new BibleTextReader(_optionsService.EpubPath))
            {
                var formattingOptions = new FormattingOptions
                {
                    IncludeVerseNumbers = false,
                    ShowBreakInVerses = false,
                    TrimPunctuation = _optionsService.TrimPunctuation,
                    TrimQuotes = _optionsService.TrimQuotes,
                    UseTildeSeparator = _optionsService.UseTildeMarker
                };

                var verses = reader.ExtractVerseTextArray(
                    bookNumber,
                    chapterAndVersesString,
                    formattingOptions);

                var epubPath = _optionsService.EpubPath;

                foreach (var vs in verses)
                {
                    var modifiedText = _verseEditorService.Get(epubPath, vs.BookNumber, vs.ChapterNumber, vs.VerseNumber);

                    var verseText = new EditVerseTextViewModel
                    {
                        EpubPath = epubPath,
                        BookNumber = vs.BookNumber,
                        Chapter = vs.ChapterNumber,
                        Verse = vs.VerseNumber,
                        OriginalVerseText = vs.Text,
                        ModifiedVerseText = modifiedText ?? vs.Text
                    };

                    Verses.Add(verseText);
                }
            }
        }

        private void AddDesignTimeItems()
        {
            if (IsInDesignMode)
            {
                var mockVerseText = LoremIpsum.GetSomeMockVerses();

                for (int n = 0; n < mockVerseText.Length; ++n)
                {
                    var item = new EditVerseTextViewModel
                    {
                        BookName = @"Book",
                        BookNumber = 1,
                        Chapter = 1,
                        Verse = n + 1,
                        OriginalVerseText = mockVerseText[n],
                        ModifiedVerseText = mockVerseText[n]
                    };

                    Verses.Add(item);
                }
            }
        }
    }
}
