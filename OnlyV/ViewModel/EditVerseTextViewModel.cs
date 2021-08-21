using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace OnlyV.ViewModel
{
    internal class EditVerseTextViewModel : ViewModelBase
    {
        private string _modifiedVerseText;
        private bool _isFocused;

        public EditVerseTextViewModel()
        {
            ResetCommand = new RelayCommand(DoReset);
        }

        public string EpubPath { get; set; }

        public string BookName { get; set; }

        public int BookNumber { get; set; }

        public int Chapter { get; set; }

        public int Verse { get; set; }

        public string OriginalVerseText { get; set; }

        public string ModifiedVerseText
        {
            get => _modifiedVerseText;
            set
            {
                if (_modifiedVerseText != value)
                {
                    _modifiedVerseText = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsModified));
                }
            }
        }

        public string BookChapterAndVerse => $"{BookName} {Chapter}:{Verse}";

        public bool IsModified => OriginalVerseText.Trim() != ModifiedVerseText.Trim();

        public bool IsFocused
        {
            get => _isFocused;
            set
            {
                if (_isFocused != value)
                {
                    _isFocused = value;
                    RaisePropertyChanged();
                }
            }
        }

        public RelayCommand ResetCommand { get; set; }

        private void DoReset()
        {
            ModifiedVerseText = OriginalVerseText;
        }
    }
}
