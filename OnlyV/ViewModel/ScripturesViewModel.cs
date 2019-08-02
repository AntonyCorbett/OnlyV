
namespace OnlyV.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Windows.Input;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using OnlyV.Helpers;
    using Services.Bible;
    using VerseExtraction.Models;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class ScripturesViewModel : ViewModelBase
    {
        private readonly IBibleVersesService _bibleService;
        private readonly List<int> _selectedVerses = new List<int>();
        private MultipleVerseSelection _multipleVerseSelection;
        private int _bookNumber;
        private int _chapterNumber;

        public ScripturesViewModel(
            IBibleVersesService bibleService)
        {
            _bibleService = bibleService;
            
            InitCommands();
            UpdateBibleBooks();
        }

        public bool ContainsHebrew => BookButtonsHebrew.Any();

        public bool ContainsGreek => BookButtonsGreek.Any();

        public ObservableCollection<ButtonModel> BookButtonsHebrew { get; } =
            new ObservableCollection<ButtonModel>();

        public ObservableCollection<ButtonModel> BookButtonsGreek { get; } =
            new ObservableCollection<ButtonModel>();

        public ObservableCollection<ButtonModel> ChapterButtons { get; } =
            new ObservableCollection<ButtonModel>();

        public ObservableCollection<VerseButtonModel> VerseButtons { get; } =
            new ObservableCollection<VerseButtonModel>();

        public int BookNumber
        {
            get => _bookNumber;
            private set
            {
                if (value != _bookNumber && value >= 0 && value <= BibleBooksMetaData.NumBibleBooks)
                {
                    _bookNumber = value;
                    RaisePropertyChanged();
                    ChapterNumber = 0;
                    RaisePropertyChanged(nameof(ScriptureText));
                    UpdateChapters();

                    if (_bibleService.GetChapterCount(_bookNumber) == 1)
                    {
                        // single-chapter book, so automatically select the chapter...
                        ChapterNumber = 1;
                        ChapterButtons[0].Selected = true;
                    }
                }
            }
        }

        public string ScriptureText => GenerateScriptureTextString();

        public string ChapterAndVersesString => GenerateChapterAndVersesString();

        private RelayCommand<object> BibleBookCommand { get; set; }

        private RelayCommand<object> ChapterCommand { get; set; }

        private RelayCommand<object> VerseCommand { get; set; }

        private int ChapterNumber
        {
            get => _chapterNumber;
            set
            {
                if (value != _chapterNumber && value >= 0)
                {
                    _chapterNumber = value;
                    RaisePropertyChanged();
                    _selectedVerses.Clear();
                    RaisePropertyChanged(nameof(ScriptureText));
                    UpdateVerses();
                }
            }
        }

        public void HandleEpubChanged()
        {
            var origBookNumber = _bookNumber;
            var origChapter = _chapterNumber;
            var origVerses = new List<int>();
            origVerses.AddRange(_selectedVerses);

            BookNumber = 0;
            UpdateBibleBooks();

            RestoreSelection(origBookNumber, origChapter, origVerses);
        }

        public bool ValidScripture()
        {
            return BookNumber > 0 && ChapterNumber > 0 && _selectedVerses.Any();
        }

        private string GenerateScriptureTextString()
        {
            var b = GetBookButton(BookNumber);
            if (b != null)
            {
                var chapterAndVs = GenerateChapterAndVersesString();
                if (chapterAndVs != null)
                {
                    var sb = new StringBuilder(b.Content);
                    sb.Append(chapterAndVs);
                    return sb.ToString();
                }
            }

            return null;
        }

        private string GenerateChapterAndVersesString()
        {
            if (ChapterNumber > 0)
            {
                var sb = new StringBuilder(ChapterNumber);

                sb.Append(" ");
                sb.Append(ChapterNumber);

                if (_selectedVerses != null)
                {
                    sb.Append(":");
                    sb.Append(VersesAsString());
                }
                
                return sb.ToString();
            }

            return null;
        }

        private ButtonModel GetFirstBookButton()
        {
            return BookButtonsHebrew.FirstOrDefault() ??
                   BookButtonsGreek.FirstOrDefault();
        }

        private ButtonModel GetBookButton(int bookNumber)
        {
            if (bookNumber > 0)
            {
                if (bookNumber <= BibleBooksMetaData.NumBibleBooksHebrew)
                {
                    if (bookNumber > BookButtonsHebrew.Count)
                    {
                        return null;
                    }

                    return BookButtonsHebrew[bookNumber - 1];
                }

                if (bookNumber <= BibleBooksMetaData.NumBibleBooks)
                {
                    if (bookNumber - BibleBooksMetaData.NumBibleBooksHebrew > BookButtonsGreek.Count)
                    {
                        return null;
                    }

                    return BookButtonsGreek[bookNumber - BibleBooksMetaData.NumBibleBooksHebrew - 1];
                }
            }

            return null;
        }

        private void UpdateBibleBooks()
        {
            ClearBookButtons();
           
            var booksData = _bibleService.GetBookData();
            if (booksData == null)
            {
                return;
            }

            int numBooks = booksData.Count;
            if (numBooks != BibleBooksMetaData.NumBibleBooks && numBooks != BibleBooksMetaData.NumBibleBooksGreek)
            {
                throw new Exception($"Found {numBooks} books. Expecting {BibleBooksMetaData.NumBibleBooks}");
            }

            int count = numBooks == BibleBooksMetaData.NumBibleBooksGreek
                ? BibleBooksMetaData.NumBibleBooksHebrew
                : 0;

            foreach (var book in booksData)
            {
                var model = new ButtonModel(book.AbbreviatedName, count + 1, BibleBookCommand);

                if (count < BibleBooksMetaData.NumBibleBooksHebrew)
                {
                    BookButtonsHebrew.Add(model);
                }
                else
                {
                    BookButtonsGreek.Add(model);
                }

                ++count;
            }
            
            BookNumber = 0;

            RaisePropertyChanged(nameof(ContainsHebrew));
            RaisePropertyChanged(nameof(ContainsGreek));
        }

        private void UpdateChapters()
        {
            ChapterButtons.Clear();

            int chapters = _bibleService.GetChapterCount(BookNumber);
            for (int n = 0; n < chapters; ++n)
            {
                ChapterButtons.Add(new ButtonModel((n + 1).ToString(), n + 1, ChapterCommand));
            }
        }

        private void ClearBookButtons()
        {
            BookButtonsGreek.Clear();
            BookButtonsHebrew.Clear();
        }

        private void OnSelectBibleBook(object commandParameter)
        {
            BookNumber = (int)commandParameter;
        }

        private bool CanSelectBook(object arg)
        {
            return true;
        }

        private void InitCommands()
        {
            BibleBookCommand = new RelayCommand<object>(OnSelectBibleBook, CanSelectBook);
            ChapterCommand = new RelayCommand<object>(OnSelectChapter, CanSelectChapter);
            VerseCommand = new RelayCommand<object>(OnSelectVerse, CanSelectVerse);
        }

        private bool CanSelectVerse(object arg)
        {
            return true;
        }

        private bool IsKeyboardShiftDown()
        {
            return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        }

        private void OnSelectVerse(object commandParameter)
        {
            var verse = (int)commandParameter;

            if (IsKeyboardShiftDown() && _multipleVerseSelection != null)
            {
                HandleMultipleVerseSelection(verse);
            }
            else
            {
                _multipleVerseSelection = null;

                var alreadySelected = _selectedVerses.Contains(verse);
                if (!alreadySelected)
                {
                    _multipleVerseSelection = new MultipleVerseSelection(verse);
                }
            }

            UpdateSelectedVerses();
        }

        private void HandleMultipleVerseSelection(int verse)
        {
            Debug.Assert(_multipleVerseSelection != null, "_multipleVerseSelection != null");
            _multipleVerseSelection.ChangeSelection(verse, VerseButtons);
        }

        private void UpdateSelectedVerses()
        {
            foreach (var b in VerseButtons)
            {
                var verse = (int)b.CommandParameter;
                var alreadySelected = _selectedVerses.Contains(verse);

                if (b.Selected != alreadySelected)
                {
                    if (alreadySelected)
                    {
                        _selectedVerses.Remove(verse);
                    }
                    else
                    {
                        _selectedVerses.Add(verse);
                    }
                }
            }

            RaisePropertyChanged(nameof(ScriptureText));
        }

        private bool CanSelectChapter(object arg)
        {
            return true;
        }

        private void OnSelectChapter(object commandParameter)
        {
            ChapterNumber = (int)commandParameter;
        }
        
        private string VersesAsString()
        {
            var sb = new StringBuilder();

            var verses = new List<int>(_selectedVerses);
            verses.Sort();

            var ranges = new List<VerseRange>();

            VerseRange current = null;
            foreach (var vs in verses)
            {
                if (current == null)
                {
                    current = new VerseRange
                    {
                        FirstVerse = vs,
                        LastVerse = vs
                    };
                }
                else
                {
                    if (vs == current.LastVerse + 1)
                    {
                        current.LastVerse = vs;
                    }
                    else
                    {
                        ranges.Add(current);
                        current = new VerseRange
                        {
                            FirstVerse = vs,
                            LastVerse = vs
                        };
                    }
                }
            }

            if (current != null)
            {
                ranges.Add(current);
            }

            foreach (var range in ranges)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                sb.Append(range.FirstVerse);
                if (range.FirstVerse != range.LastVerse)
                {
                    sb.Append(range.LastVerse == range.FirstVerse + 1 ? "," : "-");
                    sb.Append(range.LastVerse);
                }
            }

            return sb.ToString();
        }

        private void UpdateVerses()
        {
            _multipleVerseSelection = null;

            VerseButtons.Clear();

            var verseRange = _bibleService.GetVerseRange(BookNumber, ChapterNumber);
            if (verseRange != null)
            {
                for (int n = verseRange.FirstVerse; n <= verseRange.LastVerse; ++n)
                {
                    VerseButtons.Add(new VerseButtonModel(n.ToString(), n, VerseCommand));
                }
            }
        }

        private void RestoreSelection(
            int origBookNumber, int origChapter, IReadOnlyCollection<int> origVerses)
        {
            if (origBookNumber > 0)
            {
                var verses = new List<int>();
                if (origVerses != null)
                {
                    verses.AddRange(origVerses);
                }

                var bookButton = GetBookButton(origBookNumber);
                if (bookButton == null)
                {
                    // e.g. if we switched to a Bible that contains Greek Scrips only.
                    bookButton = GetFirstBookButton();
                    origBookNumber = (int)bookButton.CommandParameter;
                    origChapter = 1;
                    verses.Clear();
                    verses.Add(1);
                }

                bookButton.Selected = true;
                BookNumber = origBookNumber;

                if (origChapter > 0)
                {
                    ChapterButtons[origChapter - 1].Selected = true;
                    ChapterNumber = origChapter;

                    if (verses.Any())
                    {
                        foreach (var vs in verses)
                        {
                            VerseButtons[vs - 1].Selected = true;
                        }

                        UpdateSelectedVerses();
                    }
                }
            }
        }
    }
}
