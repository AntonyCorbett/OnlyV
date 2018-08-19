namespace OnlyV.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using OnlyV.Services;
    using OnlyV.VerseExtraction.Models;

    internal class ScripturesViewModel : ViewModelBase
    {
        private readonly IBibleVersesService _bibleService;
        private int _bookNumber;
        private int _chapterNumber;
        private ObservableCollection<int> _selectedVerses = new ObservableCollection<int>();

        public ScripturesViewModel(IBibleVersesService bibleService)
        {
            _bibleService = bibleService;
            InitCommands();
            UpdateBibleBooks();
        }

        public ObservableCollection<ButtonModel> BookButtonsHebrew { get; } =
            new ObservableCollection<ButtonModel>();

        public ObservableCollection<ButtonModel> BookButtonsGreek { get; } =
            new ObservableCollection<ButtonModel>();

        public RelayCommand<object> BibleBookCommand { get; set; }

        public int BookNumber
        {
            get => _bookNumber;
            set
            {
                if (value != _bookNumber && value >= 0 && value <= BibleBooksMetaData.NumBibleBooks)
                {
                    _bookNumber = value;
                    RaisePropertyChanged();
                    ChapterNumber = 0;
                    RaisePropertyChanged(nameof(ScriptureText));
                }
            }
        }

        public int ChapterNumber
        {
            get => _chapterNumber;
            set
            {
                if (value != _chapterNumber && value >= 0)
                {
                    _chapterNumber = value;
                    RaisePropertyChanged();
                    SelectedVerses.Clear();
                    RaisePropertyChanged(nameof(ScriptureText));
                }
            }
        }

        public ObservableCollection<int> SelectedVerses
        {
            get => _selectedVerses;
            set
            {
                _selectedVerses = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ScriptureText));
            }
        }

        public string ScriptureText => GenerateScriptureTextString();

        private string GenerateScriptureTextString()
        {
            var b = GetBookButton(BookNumber);
            if (b != null)
            {
                var sb = new StringBuilder(b.Content);
                if (ChapterNumber > 0)
                {
                    sb.Append(" ");
                    sb.Append(ChapterNumber);

                    if (SelectedVerses != null)
                    {
                        sb.Append(":");
                        sb.Append(VersesAsString(SelectedVerses));
                    }
                }

                return sb.ToString();
            }

            return null;
        }

        private ButtonModel GetBookButton(int bookNumber)
        {
            if (bookNumber > 0)
            {
                if (bookNumber <= BibleBooksMetaData.NumBibleBooksHebrew)
                {
                    return BookButtonsHebrew[bookNumber - 1];
                }

                if (bookNumber <= BibleBooksMetaData.NumBibleBooks)
                {
                    return BookButtonsGreek[bookNumber - BibleBooksMetaData.NumBibleBooksHebrew - 1];
                }
            }

            return null;
        }

        private void UpdateBibleBooks()
        {
            var booksData = _bibleService.GetBookData();

            int numBooks = booksData.Count;
            if (numBooks != BibleBooksMetaData.NumBibleBooks && numBooks != BibleBooksMetaData.NumBibleBooksGreek)
            {
                throw new Exception($"Found {numBooks} books. Expecting {BibleBooksMetaData.NumBibleBooks}");
            }

            ClearBookButtons();

            int count = numBooks == BibleBooksMetaData.NumBibleBooksGreek
                ? BibleBooksMetaData.NumBibleBooksHebrew
                : 0;

            foreach (var book in booksData)
            {
                var model = new ButtonModel(book.Name, count + 1, BibleBookCommand);

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
            // todo:
            return true;
        }

        private void InitCommands()
        {
            BibleBookCommand = new RelayCommand<object>(OnSelectBibleBook, CanSelectBook);
        }

        // todo: move into another class
        private string VersesAsString(ObservableCollection<int> selectedVerses)
        {
            var sb = new StringBuilder();

            var verses = new List<int>(SelectedVerses);
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
    }
}
