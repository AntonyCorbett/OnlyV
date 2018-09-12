namespace OnlyV.Services.Images
{
    using System.Collections.Generic;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    internal interface IImagesService
    {
        int ImageCount { get; }

        void Init(string epubPath, string themePath, int bookNumber, string chapterAndVerses);

        ImageSource Get(int index);

        IReadOnlyCollection<BitmapSource> Get();
    }
}
