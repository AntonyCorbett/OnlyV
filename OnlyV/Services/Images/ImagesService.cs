namespace OnlyV.Services.Images
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using ImageCreation;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ImagesService : IImagesService
    {
        private BitmapSource[] _images;

        public int ImageCount => _images?.Length ?? 0;

        public void Init(string epubPath, int bookNumber, string chapterAndVerses)
        {
            var bibleTextImage = new BibleTextImage();

            _images = epubPath != null 
                ? bibleTextImage.Generate(epubPath, bookNumber, chapterAndVerses).ToArray() 
                : null;
        }

        public ImageSource Get(int index)
        {
            if (index >= ImageCount)
            {
                return null;
            }

            return _images[index];
        }

        public IReadOnlyCollection<BitmapSource> Get()
        {
            return _images;
        }
    }
}
