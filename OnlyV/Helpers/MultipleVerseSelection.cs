using System.Collections.ObjectModel;
using OnlyV.ViewModel;

namespace OnlyV.Helpers
{
    internal class MultipleVerseSelection
    {
        public MultipleVerseSelection(int anchorVerse)
        {
            AnchorVerse = anchorVerse;
        }

        public int AnchorVerse { get; }

        public int LowVerse { get; private set; }

        public int HighVerse { get; private set; }

        public void ChangeSelection(int verseSelected, ObservableCollection<VerseButtonModel> verseButtons)
        {
            AddOrRemoveSelection(LowVerse, HighVerse, verseButtons, shouldAdd: false);

            LowVerse = verseSelected < AnchorVerse
                ? verseSelected
                : AnchorVerse;

            HighVerse = verseSelected > AnchorVerse
                ? verseSelected
                : AnchorVerse;

            AddOrRemoveSelection(LowVerse, HighVerse, verseButtons, shouldAdd: true);
        }

        private static void AddOrRemoveSelection(
            int startVerse, 
            int endVerse, 
            ObservableCollection<VerseButtonModel> verseButtons,
            bool shouldAdd)
        {
            if (startVerse == 0 || endVerse == 0)
            {
                return;
            }

            for (var vs = startVerse; vs <= endVerse; ++vs)
            {
                verseButtons[vs - 1].Selected = shouldAdd;
            }
        }
    }
}
