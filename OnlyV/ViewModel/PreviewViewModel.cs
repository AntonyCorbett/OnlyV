namespace OnlyV.ViewModel
{
    using System.Windows.Media;
    using GalaSoft.MvvmLight;
    using OnlyV.Services.Images;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class PreviewViewModel : ViewModelBase
    {
        private readonly IImagesService _imagesService;
        private ImageSource _previewImageSource;
        private int? _imageIndex;

        public PreviewViewModel(IImagesService imagesService)
        {
            _imagesService = imagesService;
        }

        public ImageSource PreviewImageSource
        {
            get => _previewImageSource;
            set
            {
                if (!Equals(_previewImageSource, value))
                {
                    _previewImageSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int? ImageIndex
        {
            get => _imageIndex;
            set
            {
                if (_imageIndex != value)
                {
                    _imageIndex = value;

                    PreviewImageSource = value == null 
                        ? null 
                        : _imagesService.Get(value.Value);

                    RaisePropertyChanged();
                }
            }
        }
    }
}
