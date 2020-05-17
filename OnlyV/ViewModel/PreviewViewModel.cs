using OnlyV.ImageCreation.Exceptions;

namespace OnlyV.ViewModel
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using Helpers;
    using MaterialDesignThemes.Wpf;
    using OnlyV.Services.VerseEditor;
    using Serilog;
    using Services.DisplayWindow;
    using Services.Images;
    using Services.Options;
    using Services.Snackbar;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class PreviewViewModel : ViewModelBase
    {
        private readonly IImagesService _imagesService;
        private readonly IDisplayWindowService _displayWindowService;
        private readonly IOptionsService _optionsService;
        private readonly ISnackbarService _snackbarService;
        private readonly IVerseEditorService _verseEditorService;

        private ImageSource _previewImageSource;
        private int? _imageIndex;

        public PreviewViewModel(
            IImagesService imagesService,
            IDisplayWindowService displayWindowService,
            IOptionsService optionsService,
            ISnackbarService snackbarService,
            IVerseEditorService verseEditorService)
        {
            _imagesService = imagesService;
            _displayWindowService = displayWindowService;
            _optionsService = optionsService;
            _snackbarService = snackbarService;
            _verseEditorService = verseEditorService;

            _optionsService.MediaMonitorChangedEvent += HandleMediaMonitorChangedEvent;
            _optionsService.StyleChangedEvent += HandleStyleChangedEvent;
            _optionsService.ThemePathChangedEvent += HandleThemePathChangedEvent;

            InitCommands();
        }

        public event EventHandler EditTextCommandEvent;

        public string PreviewDescription
        {
            get
            {
                var imageCount = _imagesService.ImageCount;

                if (imageCount == 1)
                {
                    return string.Format(Properties.Resources.ONE_IMAGE_GENERATED);
                }

                var sb = new StringBuilder();
                sb.Append(string.Format(Properties.Resources.X_IMAGES_GENERATED, _imagesService.ImageCount));
                sb.Append(" ");
                sb.Append(Properties.Resources.DRAG_INTO_ONLYM);
                return sb.ToString();
            }
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

                    _displayWindowService.SetImage(PreviewImageSource);

                    RaisePropertyChanged();
                }
            }
        }

        public string DisplayButtonToolTip =>
            IsDisplayWindowVisible
                ? Properties.Resources.HIDE_CMD
                : Properties.Resources.DISPLAY_CMD;

        public string BookChapterAndVersesString { get; set; }

        public bool IsDisplayWindowVisible => _displayWindowService.IsWindowVisible;

        public bool AllowVerseEditing => _optionsService.AllowVerseEditing;

        public RelayCommand NextImageCommand { get; set; }

        public RelayCommand PreviousImageCommand { get; set; }

        public RelayCommand DisplayImageCommand { get; set; }

        public RelayCommand SaveCommand { get; set; }

        public RelayCommand CopyToClipboardCommand { get; set; }

        public RelayCommand EditTextCommand { get; set; }

        public PackIcon EditBadgePackIcon => VerseTextIsModified()
            ? new PackIcon { Kind = PackIconKind.Check }
            : null;
    
        // returns the name of the saved file (if only 1), or the
        // name of the folder in which multiple files are stored
        public string SaveImagesToFolder(string folder)
        {
            var imagesToSave = _imagesService.Get();
            if (imagesToSave == null || !imagesToSave.Any())
            {
                return null;
            }

            var s = new ImageSavingService(
                imagesToSave,
                folder,
                BookChapterAndVersesString);
            try
            {
                return s.Execute();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, @"Could not save");
                return null;
            }
        }

        public string GetSuitableFileNameWithoutExtension()
        {
            return ImageSavingService.GetSuitableFilenameWithoutExtension(BookChapterAndVersesString);
        }

        private void InitCommands()
        {
            NextImageCommand = new RelayCommand(ShowNextImage, CanShowNextImage);
            PreviousImageCommand = new RelayCommand(ShowPreviousImage, CanShowPreviousImage);
            DisplayImageCommand = new RelayCommand(ToggleDisplayImage, CanToggleDisplayImage);
            SaveCommand = new RelayCommand(SaveImage, CanSaveImage);
            CopyToClipboardCommand = new RelayCommand(CopyToClipboard, CanCopyToClipboard);
            EditTextCommand = new RelayCommand(EditText, CanEditText);
        }

        private bool CanEditText()
        {
            return true;
        }

        private void EditText()
        {
            // display the "edit verse text" page...
            EditTextCommandEvent?.Invoke(this, EventArgs.Empty);
        }

        private bool CanCopyToClipboard()
        {
            return true;
        }

        private void CopyToClipboard()
        {
            Clipboard.Clear();
            Clipboard.SetImage((BitmapSource)PreviewImageSource);

            _snackbarService.EnqueueWithOk(Properties.Resources.COPIED);
        }

        private bool CanSaveImage()
        {
            return true;
        }

        private void SaveImage()
        {
            var folder = GetDestinationFolder();

            var imagesToSave = _imagesService.Get();
            if (imagesToSave == null || !imagesToSave.Any())
            {
                Log.Logger.Error(@"No images to save!");
                _snackbarService.EnqueueWithOk(Properties.Resources.NO_IMAGES);
                return;
            }

            var s = new ImageSavingService(
                imagesToSave,
                folder, 
                BookChapterAndVersesString);

            try
            {
                s.Execute();

                var msg = imagesToSave.Count > 1
                    ? string.Format(Properties.Resources.SAVED_X_IMAGES, imagesToSave.Count)
                    : Properties.Resources.SAVED_IMAGE;

                _snackbarService.Enqueue(msg, Properties.Resources.VIEW, LaunchFileExplorer);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, @"Could not save");
                _snackbarService.EnqueueWithOk(Properties.Resources.ERROR_SAVING);
            }
        }

        private void LaunchFileExplorer()
        {
            Process.Start(GetDestinationFolder());
        }

        private string GetDestinationFolder()
        {
            if (FileUtils.DirectoryIsAvailable(_optionsService.SaveToFolder))
            {
                return _optionsService.SaveToFolder;
            }

            var folder = FileUtils.GetDefaultSaveToFolder();
            FileUtils.CreateDirectory(folder);

            if (!FileUtils.DirectoryIsAvailable(folder))
            {
                throw new Exception($@"Folder is not available: {folder}");
            }

            return folder;
        }

        private void ToggleDisplayImage()
        {
            _displayWindowService.ToggleWindow();

            if (IsDisplayWindowVisible)
            {
                _displayWindowService.SetImage(_imagesService.Get(_imageIndex ?? 0));
            }

            RaisePropertyChanged(nameof(IsDisplayWindowVisible));
            RaisePropertyChanged(nameof(DisplayButtonToolTip));

            // retain focus and so allow F5 to work.
            Application.Current?.MainWindow?.Focus();
        }

        private bool CanToggleDisplayImage()
        {
            if (IsDisplayWindowVisible)
            {
                return true;
            }

            return _optionsService.MediaMonitorId != null;
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

        private void HandleMediaMonitorChangedEvent(object sender, OnlyV.EventArgs.MonitorChangedEventArgs e)
        {
            _displayWindowService.ChangeTargetMonitor();
        }

        private void HandleStyleChangedEvent(object sender, EventArgs e)
        {
            RefreshImages();
        }

        private void HandleThemePathChangedEvent(object sender, EventArgs e)
        {
            RefreshImages();
        }

        private void RefreshImages()
        {
            try
            {
                // when the style or theme is changed in the settings page we must
                // ensure we display the first image (the style settings may change
                // the number of images available).
                ImageIndex = null;
                _imagesService.Refresh();
                ImageIndex = 0;
            }
            catch (TextTooLargeException ex)
            {
                _snackbarService.EnqueueWithOk(ex.Message);
            }
            catch (Exception)
            {
                _snackbarService.EnqueueWithOk(Properties.Resources.UNKNOWN_ERROR);
            }
        }

        private bool VerseTextIsModified()
        {
            return _imagesService.VerseTextIsModified;
        }
    }
}
