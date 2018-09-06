namespace OnlyV.Services.Images
{
    using System;
    using System.Linq;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using OnlyV.ImageCreation;

    internal class ImagesService : IImagesService
    {
        private BitmapSource[] _images;

        public int ImageCount => _images?.Length ?? 0;

        public void Init(string epubPath, int bookNumber, string chapterAndVerses)
        {
            var bibleTextImage = new BibleTextImage();
            _images = bibleTextImage.Generate(epubPath, bookNumber, chapterAndVerses).ToArray();
        }

        public ImageSource Get(int index)
        {
            if (index >= ImageCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _images[index];
        }
    }
}
