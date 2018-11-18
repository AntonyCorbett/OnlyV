namespace OnlyV.ViewModel
{
    using System.Collections.ObjectModel;
    using GalaSoft.MvvmLight;
    using OnlyV.Helpers;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class EditTextViewModel : ViewModelBase
    {
        public EditTextViewModel()
        {
            AddDesignTimeItems();
        }

        public ObservableCollection<EditVerseTextViewModel> Verses { get; } = new ObservableCollection<EditVerseTextViewModel>();

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
                        Text = mockVerseText[n]
                    };

                    Verses.Add(item);
                }
            }
        }
    }
}
