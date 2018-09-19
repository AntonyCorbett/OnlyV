namespace OnlyVThemeCreator.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using GalaSoft.MvvmLight.Messaging;
    using GalaSoft.MvvmLight.Threading;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using OnlyV.ImageCreation;
    using OnlyV.Themes.Common;
    using OnlyV.Themes.Common.Extensions;
    using OnlyV.Themes.Common.FileHandling;
    using OnlyV.Themes.Common.Models;
    using OnlyV.Themes.Common.Services;
    using OnlyV.Themes.Common.Services.UI;
    using OnlyV.Themes.Common.Specs;
    using OnlyV.VerseExtraction;
    using OnlyVThemeCreator.Helpers;
    using OnlyVThemeCreator.PubSubMessages;
    using OnlyVThemeCreator.Services;

    public class MainViewModel : ViewModelBase
    {
        private readonly IUserInterfaceService _userInterfaceService;
        private readonly IOptionsService _optionsService;
        private readonly BibleTextImage _imageService;
        private readonly SingleExecAction _singleExecAction = new SingleExecAction(TimeSpan.FromMilliseconds(500));
        private int _currentSampleTextId;
        private ImageSource _imageSource;
        private OnlyVTheme _currentTheme;
        private BitmapImage _backgroundImage;
        private Color _defaultBackgroundColor = Colors.Blue;
        private Color _defaultTextColor = Colors.White;
        private bool _isSampleBackgroundImageUsed;
        
        public MainViewModel(
            IUserInterfaceService userInterfaceService,
            IOptionsService optionsService)
        {
            _userInterfaceService = userInterfaceService;
            _optionsService = optionsService;
            _imageService = new BibleTextImage();

            InitCommands();

            _currentTheme = new OnlyVTheme();
            _isSampleBackgroundImageUsed = true;

            BibleEpubFiles = GetBibleEpubFiles();

            Messenger.Default.Register<DragOverMessage>(this, OnDragOver);
            Messenger.Default.Register<DragDropMessage>(this, OnDragDrop);
        }

        public event EventHandler EpubChangedEvent;

        public event EventHandler SampleTextChangedEvent;

        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            {
                if (_imageSource != value)
                {
                    _imageSource = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsSampleImageAvailable));
                }
            }
        }

        public OnlyVFontStyle[] FontStyles { get; } = new OnlyVFontStyle[]
        {
            OnlyVFontStyle.Normal,
            OnlyVFontStyle.Italic,
            OnlyVFontStyle.Oblique
        };

        public OnlyVFontWeight[] FontWeights { get; } = new OnlyVFontWeight[]
        {
            OnlyVFontWeight.Light,
            OnlyVFontWeight.Normal,
            OnlyVFontWeight.SemiBold,
            OnlyVFontWeight.Bold
        };

        public bool IsSampleImageAvailable => ImageSource != null;

        public bool IsSampleBackgroundImageUsed
        {
            get => _isSampleBackgroundImageUsed;
            set
            {
                if (_isSampleBackgroundImageUsed != value)
                {
                    _isSampleBackgroundImageUsed = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public IEnumerable<EpubFileItem> BibleEpubFiles { get; }

        public string CurrentEpubFilePath
        {
            get => _optionsService.EpubPath;
            set
            {
                if (_optionsService.EpubPath != value)
                {
                    _optionsService.EpubPath = value;
                    RaisePropertyChanged();

                    using (_userInterfaceService.GetBusy())
                    {
                        UpdateTextSamples();

                        UpdateImage();
                        EpubChangedEvent?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        public int CurrentSampleTextId
        {
            get => _currentSampleTextId;
            set
            {
                if (_currentSampleTextId != value)
                {
                    _currentSampleTextId = value;
                    RaisePropertyChanged();
                    
                    UpdateImage();
                    SampleTextChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public ObservableCollection<VerseTextItem> TextSamples { get; } = new ObservableCollection<VerseTextItem>();

        public bool IsBackgroundImageSpecified => _backgroundImage != null;

        public BitmapImage BackgroundImage
        {
            get => _backgroundImage;
            set
            {
                if (_backgroundImage != value)
                {
                    _backgroundImage = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsBackgroundImageSpecified));

                    UpdateImage();
                }
            }
        }
        
        public int Height
        {
            get => _currentTheme.Dimensions.ImageHeight;
            set
            {
                if (_currentTheme.Dimensions.ImageHeight != value)
                {
                    _currentTheme.Dimensions.ImageHeight = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public int Width
        {
            get => _currentTheme.Dimensions.ImageWidth;
            set
            {
                if (_currentTheme.Dimensions.ImageWidth != value)
                {
                    _currentTheme.Dimensions.ImageWidth = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public int LeftMargin
        {
            get => _currentTheme.Dimensions.LeftMargin;
            set
            {
                if (_currentTheme.Dimensions.LeftMargin != value)
                {
                    _currentTheme.Dimensions.LeftMargin = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public int TopMargin
        {
            get => _currentTheme.Dimensions.TopMargin;
            set
            {
                if (_currentTheme.Dimensions.TopMargin != value)
                {
                    _currentTheme.Dimensions.TopMargin = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public int RightMargin
        {
            get => _currentTheme.Dimensions.RightMargin;
            set
            {
                if (_currentTheme.Dimensions.RightMargin != value)
                {
                    _currentTheme.Dimensions.RightMargin = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public int BottomMargin
        {
            get => _currentTheme.Dimensions.BottomMargin;
            set
            {
                if (_currentTheme.Dimensions.BottomMargin != value)
                {
                    _currentTheme.Dimensions.BottomMargin = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public Color? BackgroundColour
        {
            get => ConvertFromString(_currentTheme.Background.Colour, _defaultBackgroundColor);
            set
            {
                if (value == null)
                {
                    value = _defaultBackgroundColor;
                }

                if (ConvertFromString(_currentTheme.Background.Colour, _defaultBackgroundColor) != value)
                {
                    _currentTheme.Background.Colour = value.Value.ToString();
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public string BodyTextFontFamilyName
        {
            get => _currentTheme.BodyText.Font.Family;
            set
            {
                if (_currentTheme.BodyText.Font.Family != value)
                {
                    _currentTheme.BodyText.Font.Family = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public double BodyTextSize
        {
            get => _currentTheme.BodyText.Font.Size;
            set
            {
                if (_currentTheme.BodyText.Font.Size != value)
                {
                    _currentTheme.BodyText.Font.Size = value;

                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public double BodyTextOpacity
        {
            get => _currentTheme.BodyText.Font.Opacity;
            set
            {
                if (_currentTheme.BodyText.Font.Opacity != value && IsValidOpacity(value))
                {
                    _currentTheme.BodyText.Font.Opacity = value;

                    _singleExecAction.Execute(() =>
                    {
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            RaisePropertyChanged();
                            UpdateImage();
                        });
                    });
                }
            }
        }

        public OnlyVFontStyle BodyTextFontStyle
        {
            get => _currentTheme.BodyText.Font.Style;
            set
            {
                if (_currentTheme.BodyText.Font.Style != value)
                {
                    _currentTheme.BodyText.Font.Style = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public OnlyVFontWeight BodyTextFontWeight
        {
            get => _currentTheme.BodyText.Font.Weight;
            set
            {
                if (_currentTheme.BodyText.Font.Weight != value)
                {
                    _currentTheme.BodyText.Font.Weight = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public Color? BodyTextColour
        {
            get => ConvertFromString(_currentTheme.BodyText.Font.Colour, _defaultTextColor);
            set
            {
                if (value == null)
                {
                    value = _defaultTextColor;
                }

                if (ConvertFromString(_currentTheme.BodyText.Font.Colour, _defaultTextColor) != value)
                {
                    _currentTheme.BodyText.Font.Colour = value.Value.ToString();
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public RelayCommand NewFileCommand { get; set; }

        public RelayCommand OpenFileCommand { get; set; }

        public RelayCommand SaveFileCommand { get; set; }

        public RelayCommand SaveAsFileCommand { get; set; }

        public RelayCommand ClearBackgroundImageCommand { get; set; }
        
        private void UpdateTextSamples()
        {
            var originalSampleId = CurrentSampleTextId;
            CurrentSampleTextId = 0;

            TextSamples.Clear();

            var newSamples = CreateTextSampleItems();
            foreach (var sample in CreateTextSampleItems())
            {
                TextSamples.Add(sample);
            }

            if (TextSamples.Any(x => x.Id.Equals(originalSampleId)))
            {
                CurrentSampleTextId = originalSampleId;
            }
            else
            {
                CurrentSampleTextId = TextSamples.FirstOrDefault()?.Id ?? 0;
            }
        }

        private IReadOnlyCollection<EpubFileItem> GetBibleEpubFiles()
        {
            var result = new List<EpubFileItem>();

            var files = Directory.GetFiles(FileUtils.GetEpubFolder(), "*.epub").ToList();

            foreach (var file in files)
            {
                result.Add(new EpubFileItem
                {
                    Path = file,
                    Name = Path.GetFileNameWithoutExtension(file)
                });
            }

            result.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
            return result;
        }

        private IReadOnlyCollection<VerseTextItem> CreateTextSampleItems()
        {
            var result = new List<VerseTextItem>();

            if (IsValidEpub())
            {
                using (var reader = new BibleTextReader(CurrentEpubFilePath))
                {
                    foreach (var sample in StandardTextSample.GetStandardList())
                    {
                        result.Add(new VerseTextItem(
                            sample.Id,
                            GetSampleName(reader, sample), 
                            sample.BookNumber,
                            sample.ChapterAndVerses));
                    }
                }
            }

            return result;
        }

        private bool IsValidEpub()
        {
            return !string.IsNullOrEmpty(CurrentEpubFilePath) && File.Exists(CurrentEpubFilePath);
        }

        private string GetSampleName(BibleTextReader reader, StandardTextSample sample)
        {
            return reader.GenerateVerseTitle(sample.BookNumber, sample.ChapterAndVerses);
        }

        private void UpdateImage()
        {
            using (_userInterfaceService.GetBusy())
            {
                if (CurrentSampleTextId == 0 || !IsValidEpub())
                {
                    ImageSource = null;
                    return;
                }

                var sample = TextSamples.SingleOrDefault(x => x.Id.Equals(CurrentSampleTextId));
                if (sample == null)
                {
                    ImageSource = null;
                    return;
                }

                ApplyFormatting();

                var images = _imageService.Generate(CurrentEpubFilePath, sample.BookNumber, sample.ChapterAndVerses);
                if (images == null || !images.Any())
                {
                    ImageSource = null;
                    return;
                }

                ImageSource = images.First();
            }
        }

        private void ApplyFormatting()
        {
            // dimensions...
            _imageService.Width = Width;
            _imageService.Height = Height;
            _imageService.LeftMargin = LeftMargin;
            _imageService.TopMargin = TopMargin;
            _imageService.RightMargin = RightMargin;
            _imageService.BottomMargin = BottomMargin;

            // background...
            _imageService.BackgroundImageSource = IsSampleBackgroundImageUsed 
                ? _backgroundImage 
                : null;

            _imageService.BackgroundColor = BackgroundColour ?? _defaultBackgroundColor;

            // body text...
            _imageService.MainFont.FontFamily = new FontFamily(BodyTextFontFamilyName);
            _imageService.MainFont.FontSize = BodyTextSize;
            _imageService.MainFont.FontStyle = BodyTextFontStyle.AsWindowsFontStyle();
            _imageService.MainFont.FontWeight = BodyTextFontWeight.AsWindowsFontWeight();
            _imageService.MainFont.FontColor = BodyTextColour ?? _defaultTextColor;
            _imageService.MainFont.Opacity = BodyTextOpacity;
        }

        private void InitCommands()
        {
            NewFileCommand = new RelayCommand(NewFile, CanExecuteNewFile);
            OpenFileCommand = new RelayCommand(OpenFile, CanExecuteOpenFile);
            SaveFileCommand = new RelayCommand(SaveFile, CanExecuteSaveFile);
            SaveAsFileCommand = new RelayCommand(SaveAsFile, CanExecuteSaveAsFile);
            ClearBackgroundImageCommand = new RelayCommand(ClearBackgroundImage, CanExecuteClearBackgroundImage);
        }

        private bool CanExecuteClearBackgroundImage()
        {
            return true;
        }

        private void ClearBackgroundImage()
        {
            BackgroundImage = null;
        }

        private bool CanExecuteSaveAsFile()
        {
            return true;
        }

        private void SaveAsFile()
        {
            using (var d = new CommonSaveFileDialog())
            {
                d.OverwritePrompt = true;
                d.AlwaysAppendDefaultExtension = true;
                d.IsExpandedMode = true;
                d.DefaultDirectory = FileUtils.GetThemeFolder();
                d.DefaultExtension = ThemeFile.ThemeFileExtension;
                d.Filters.Add(new CommonFileDialogFilter(Properties.Resources.THEME_FILE, $"*{ThemeFile.ThemeFileExtension}"));
                d.Title = Properties.Resources.SAVE_THEME;

                var rv = d.ShowDialog();
                if (rv == CommonFileDialogResult.Ok)
                {
                    var themeName = d.FileName;

                    ThemeFile f = new ThemeFile();
                    f.Create(themeName, _currentTheme, _backgroundImage, overwrite: true);
                }
            }
        }

        private bool CanExecuteSaveFile()
        {
            return true;
        }

        private void SaveFile()
        {
            // todo:
        }

        private bool CanExecuteOpenFile()
        {
            return true;
        }

        private void OpenFile()
        {
            // todo:
        }

        private bool CanExecuteNewFile()
        {
            return true;
        }

        private void NewFile()
        {
            // todo:
        }

        private void OnDragDrop(DragDropMessage message)
        {
            using (_userInterfaceService.GetBusy())
            {
                var file = GetDroppedFile(message.DragEventArgs);
                if (file != null)
                {
                    BackgroundImage = new BitmapImage(new Uri(file));
                }
            }
        }

        private string GetDroppedFile(DragEventArgs e)
        {
            var result = new List<string>();

            if (e.Data is DataObject dataObject && dataObject.ContainsFileDropList())
            {
                foreach (var filePath in dataObject.GetFileDropList())
                {
                    if (IsImageFile(filePath))
                    {
                        return filePath;
                    }
                }
            }

            return null;
        }

        private void OnDragOver(DragOverMessage message)
        {
            message.DragEventArgs.Effects = CanAcceptDrop(message.DragEventArgs)
                ? DragDropEffects.Copy
                : DragDropEffects.None;

            message.DragEventArgs.Handled = true;
        }

        private bool CanAcceptDrop(DragEventArgs e)
        {
            if (e.Data is DataObject dataObject && dataObject.ContainsFileDropList())
            {
                foreach (var filePath in dataObject.GetFileDropList())
                {
                    if (IsImageFile(filePath))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsImageFile(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(ext))
            {
                return false;
            }

            return
                ext.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
        }

        private Color ConvertFromString(string htmlColor, Color defaultColor)
        {
            if (string.IsNullOrEmpty(htmlColor))
            {
                return defaultColor;
            }

            try
            {
                var color = ColorConverter.ConvertFromString(htmlColor);
                return (Color?)color ?? defaultColor;
            }
            catch (FormatException)
            {
                return defaultColor;
            }
        }

        private bool IsValidOpacity(double value)
        {
            return value >= 0 && value <= 1.0;
        }
    }
}