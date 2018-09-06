namespace OnlyV.ViewModel
{
    using System.Windows.Media;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
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
            InitCommands();
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

        public RelayCommand NextImageCommand { get; set; }

        public RelayCommand PreviousImageCommand { get; set; }

        private void InitCommands()
        {
            NextImageCommand = new RelayCommand(ShowNextImage, CanShowNextImage);
            PreviousImageCommand = new RelayCommand(ShowPreviousImage, CanShowPreviousImage);
        }

        private bool CanShowPreviousImage()
        {
            return ImageIndex > 0;
        }

        private void ShowPreviousImage()
        {
            ImageIndex = ImageIndex - 1;
        }

        private bool CanShowNextImage()
        {
            return ImageIndex < _imagesService.ImageCount - 1;
        }

        private void ShowNextImage()
        {
            ImageIndex = ImageIndex + 1;
        }
    }
}
