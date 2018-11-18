namespace OnlyV.ViewModel
{
    using GalaSoft.MvvmLight;

    internal class EditVerseTextViewModel : ViewModelBase
    {
        public string BookName { get; set; }

        public int BookNumber { get; set; }

        public int Chapter { get; set; }

        public int Verse { get; set; }

        public string Text { get; set; }

        public string BookChapterAndVerse => $"{BookName} {Chapter}:{Verse}";
    }
}
