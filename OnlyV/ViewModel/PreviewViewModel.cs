namespace OnlyV.ViewModel
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using OnlyV.Helpers;
    using OnlyV.Services.Options;
    using Services.DisplayWindow;
    using Services.Images;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class PreviewViewModel : ViewModelBase
    {
        private readonly IImagesService _imagesService;
        private readonly IDisplayWindowService _displayWindowService;
        private readonly IOptionsService _optionsService;
        private ImageSource _previewImageSource;
        private int? _imageIndex;

        public PreviewViewModel(
            IImagesService imagesService,
            IDisplayWindowService displayWindowService,
            IOptionsService optionsService)
        {
            _imagesService = imagesService;
            _displayWindowService = displayWindowService;
            _optionsService = optionsService;

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
            private get => _imageIndex;
            set
            {
                if (_imageIndex != value)
                {
                    _imageIndex = value;

                    PreviewImageSource = value == null 
                        ? null 
                        : _imagesService.Get(value.Value);

                    _displayWindowService.SetImage(PreviewImageSource);

                    RaisePropertyChanged();
                }
            }
        }

        public string BookChapterAndVersesString { get; set; }

        public bool IsDisplayWindowVisible => _displayWindowService.IsWindowVisible;

        public RelayCommand NextImageCommand { get; set; }

        public RelayCommand PreviousImageCommand { get; set; }

        public RelayCommand DisplayImageCommand { get; set; }

        public RelayCommand SaveCommand { get; set; }

        private void InitCommands()
        {
            NextImageCommand = new RelayCommand(ShowNextImage, CanShowNextImage);
            PreviousImageCommand = new RelayCommand(ShowPreviousImage, CanShowPreviousImage);
            DisplayImageCommand = new RelayCommand(ToggleDisplayImage, CanToggleDisplayImage);
            SaveCommand = new RelayCommand(SaveImage, CanSaveImage);
        }

        private bool CanSaveImage()
        {
            return true;
        }

        private void SaveImage()
        {
            var folder = GetDestinationFolder(); 

            var s = new ImageSavingService(
                _imagesService.Get(),
                folder, 
                BookChapterAndVersesString);

            s.Execute();
        }

        private string GetDestinationFolder()
        {
            if (FileUtils.DirectoryIsAvailable(_optionsService.Options.SaveToFolder))
            {
                return _optionsService.Options.SaveToFolder;
            }

            var folder = FileUtils.GetDefaultSaveToFolder();
            FileUtils.CreateDirectory(folder);

            if (!FileUtils.DirectoryIsAvailable(folder))
            {
                throw new Exception($"Folder is not available: {folder}");
            }

            return folder;
        }

        private void ToggleDisplayImage()
        {
            _displayWindowService.ClearImage();

            _displayWindowService.ToggleWindow();

            if (IsDisplayWindowVisible)
            {
                _displayWindowService.SetImage(_imagesService.Get(_imageIndex ?? 0));
            }
            
            RaisePropertyChanged(nameof(IsDisplayWindowVisible));

            // retain focus and so allow F5 to work.
            Application.Current?.MainWindow?.Focus();
        }

        private bool CanToggleDisplayImage()
        {
            return true;
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
