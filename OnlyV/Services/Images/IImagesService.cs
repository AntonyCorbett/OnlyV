namespace OnlyV.Services.Images
{
    using System.Windows.Media;

    internal interface IImagesService
    {
        int ImageCount { get; }

        void Init(string epubPath, int bookNumber, string chapterAndVerses);

        ImageSource Get(int index);
    }
}
