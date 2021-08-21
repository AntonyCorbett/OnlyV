using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OnlyV.Services.Images
{
    internal interface IImagesService
    {
        int ImageCount { get; }

        bool VerseTextIsModified { get; }

        void Init(int bookNumber, string chapterAndVerses);

        void Refresh();

        ImageSource Get(int index);

        IReadOnlyCollection<BitmapSource> Get();
    }
}
