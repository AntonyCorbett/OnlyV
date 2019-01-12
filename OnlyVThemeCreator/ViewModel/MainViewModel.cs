namespace OnlyVThemeCreator.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using GalaSoft.MvvmLight.Messaging;
    using GalaSoft.MvvmLight.Threading;
    using Helpers;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using Newtonsoft.Json;
    using OnlyV.ImageCreation;
    using OnlyV.Themes.Common;
    using OnlyV.Themes.Common.Extensions;
    using OnlyV.Themes.Common.FileHandling;
    using OnlyV.Themes.Common.Models;
    using OnlyV.Themes.Common.Services;
    using OnlyV.Themes.Common.Services.UI;
    using OnlyV.Themes.Common.Specs;
    using OnlyV.VerseExtraction;
    using PubSubMessages;
    using Services;

    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MainViewModel : ViewModelBase
    {
        private const string AppName = @"O N L Y V  Theme Creator";
        private const double TextSizeTolerance = 0.1;
        private const double BlurRadiusTolerance = 0.5;
        private const double ShadowDepthTolerance = 0.5;
        private const double OpacityTolerance = 0.01;

        private readonly IUserInterfaceService _userInterfaceService;
        private readonly IOptionsService _optionsService;
        private readonly BibleTextImage _imageService;
        private readonly IDialogService _dialogService;
        private readonly SingleExecAction _singleExecAction = new SingleExecAction(TimeSpan.FromMilliseconds(500));
        private readonly Color _defaultBackgroundColor = Colors.Blue;
        private readonly Color _defaultTextColor = Colors.White;
        private readonly Color _defaultDropShadowColor = Colors.Black;
        private int _currentSampleTextId;
        private ImageSource _imageSource;
        private OnlyVTheme _currentTheme;
        private BitmapImage _backgroundImage;
        private bool _isSampleBackgroundImageUsed;
        private string _currentThemePath;
        private string _defaultFileSaveFolder;
        private string _defaultFileOpenFolder;
        private string _lastSavedThemeSignature;
        
        public MainViewModel(
            IUserInterfaceService userInterfaceService,
            IOptionsService optionsService,
            IDialogService dialogService)
        {
            _userInterfaceService = userInterfaceService;
            _optionsService = optionsService;
            _dialogService = dialogService;
            _imageService = new BibleTextImage();

            InitCommands();

            SystemFonts = GetSystemFonts();

            _currentTheme = new OnlyVTheme();
            _isSampleBackgroundImageUsed = true;

            BibleEpubFiles = GetBibleEpubFiles();

            Messenger.Default.Register<DragOverMessage>(this, OnDragOver);
            Messenger.Default.Register<DragDropMessage>(this, OnDragDrop);
            
            UpdateTextSamples();

            SaveSignature();
        }

        public event EventHandler EpubChangedEvent;

        public event EventHandler SampleTextChangedEvent;

        public double MaxBlurRadiusValue => 50;

        public double BlurRadiusLargeChange => MaxBlurRadiusValue / 10;

        public double MaxShadowDepthValue => 50;

        public double ShadowDepthLargeChange => MaxShadowDepthValue / 10;

        public SystemFont[] SystemFonts { get; }

        public bool IsDirty => !CreateThemeSignature().Equals(_lastSavedThemeSignature);
        
        public OnlyVTheme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;

                    // notifies that all properties have changed...
                    RaisePropertyChanged(string.Empty);
                }
            }
        }

        public string CurrentThemePath
        {
            get => _currentThemePath;
            set
            {
                if (_currentThemePath == null || _currentThemePath != value)
                {
                    _currentThemePath = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(MainWindowCaption));
                }
            }
        }

        public string MainWindowCaption
        {
            get
            {
                if (!string.IsNullOrEmpty(_currentThemePath))
                {
                    return $"{Path.GetFileNameWithoutExtension(_currentThemePath)} - {AppName}";
                }

                return AppName; 
            }
        }

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

        public OnlyVFontStyle[] FontStyles { get; } = 
        {
            OnlyVFontStyle.Normal,
            OnlyVFontStyle.Italic,
            OnlyVFontStyle.Oblique
        };

        public OnlyVFontWeight[] FontWeights { get; } = 
        {
            OnlyVFontWeight.Light,
            OnlyVFontWeight.Normal,
            OnlyVFontWeight.SemiBold,
            OnlyVFontWeight.Bold
        };

        public OnlyVHorizontalTextAlignment[] HorizontalAlignments { get; } = 
        {
            OnlyVHorizontalTextAlignment.Left,
            OnlyVHorizontalTextAlignment.Centre,
            OnlyVHorizontalTextAlignment.Right
        };

        public OnlyVBodyVerticalAlignment[] VerticalAlignments { get; } =
        {
            OnlyVBodyVerticalAlignment.Top,
            OnlyVBodyVerticalAlignment.Middle
        };

        public OnlyVLineSpacing[] LineSpacings { get; } = 
        {
            OnlyVLineSpacing.VerySmall,
            OnlyVLineSpacing.Small,
            OnlyVLineSpacing.Normal,
            OnlyVLineSpacing.Large,
            OnlyVLineSpacing.VeryLarge
        };

        public OnlyVTitlePosition[] TitlePositions { get; } = 
        {
            OnlyVTitlePosition.Top,
            OnlyVTitlePosition.Bottom
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

        public double BackgroundImageOpacity
        {
            get => _currentTheme.Background.ImageOpacity;
            set
            {
                SetOpacityProperty(nameof(BackgroundImageOpacity), _currentTheme.Background.ImageOpacity, value, v => _currentTheme.Background.ImageOpacity = v);
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
                if (Math.Abs(_currentTheme.BodyText.Font.Size - value) > TextSizeTolerance)
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
                SetOpacityProperty(nameof(BodyTextOpacity), _currentTheme.BodyText.Font.Opacity, value, v => _currentTheme.BodyText.Font.Opacity = v);
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

        public OnlyVHorizontalTextAlignment BodyTextHorizontalAlignment
        {
            get => _currentTheme.BodyText.HorizontalAlignment;
            set
            {
                if (_currentTheme.BodyText.HorizontalAlignment != value)
                {
                    _currentTheme.BodyText.HorizontalAlignment = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public OnlyVLineSpacing LineSpacing
        {
            get => _currentTheme.BodyText.LineSpacing;
            set
            {
                if (_currentTheme.BodyText.LineSpacing != value)
                {
                    _currentTheme.BodyText.LineSpacing = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public bool BodyTextDropShadow
        {
            get => _currentTheme.BodyText.DropShadow.Show;
            set
            {
                if (_currentTheme.BodyText.DropShadow.Show != value)
                {
                    _currentTheme.BodyText.DropShadow.Show = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public OnlyVBodyVerticalAlignment BodyVerticalAlignment
        {
            get => _currentTheme.BodyText.BodyVerticalAlignment;
            set
            {
                if (_currentTheme.BodyText.BodyVerticalAlignment != value)
                {
                    _currentTheme.BodyText.BodyVerticalAlignment = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public Color? BodyDropShadowColour
        {
            get => ConvertFromString(_currentTheme.BodyText.DropShadow.Colour, _defaultDropShadowColor);
            set
            {
                if (value == null)
                {
                    value = _defaultDropShadowColor;
                }

                if (ConvertFromString(_currentTheme.BodyText.DropShadow.Colour, _defaultDropShadowColor) != value)
                {
                    _currentTheme.BodyText.DropShadow.Colour = value.Value.ToString();
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public double BodyDropShadowOpacity
        {
            get => _currentTheme.BodyText.DropShadow.Opacity;
            set
            {
                SetOpacityProperty(nameof(BodyDropShadowOpacity), _currentTheme.BodyText.DropShadow.Opacity, value, v => _currentTheme.BodyText.DropShadow.Opacity = v);
            }
        }

        public double BodyDropShadowBlurRadius
        {
            get => _currentTheme.BodyText.DropShadow.BlurRadius;
            set
            {
                if (Math.Abs(_currentTheme.BodyText.DropShadow.BlurRadius - value) > BlurRadiusTolerance && 
                    IsValidBlurRadius(value))
                {
                    _currentTheme.BodyText.DropShadow.BlurRadius = value;

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

        public double BodyDropShadowDepth
        {
            get => _currentTheme.BodyText.DropShadow.Depth;
            set
            {
                if (Math.Abs(_currentTheme.BodyText.DropShadow.Depth - value) > ShadowDepthTolerance && 
                    IsValidShadowDepth(value))
                {
                    _currentTheme.BodyText.DropShadow.Depth = value;

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

        public string TitleTextFontFamilyName
        {
            get => _currentTheme.TitleText.Font.Family;
            set
            {
                if (_currentTheme.TitleText.Font.Family != value)
                {
                    _currentTheme.TitleText.Font.Family = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public double TitleTextSize
        {
            get => _currentTheme.TitleText.Font.Size;
            set
            {
                if (Math.Abs(_currentTheme.TitleText.Font.Size - value) > TextSizeTolerance)
                {
                    _currentTheme.TitleText.Font.Size = value;

                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public double TitleTextOpacity
        {
            get => _currentTheme.TitleText.Font.Opacity;
            set
            {
                SetOpacityProperty(nameof(TitleTextOpacity), _currentTheme.TitleText.Font.Opacity, value, v => _currentTheme.TitleText.Font.Opacity = v);
            }
        }

        public OnlyVFontStyle TitleTextFontStyle
        {
            get => _currentTheme.TitleText.Font.Style;
            set
            {
                if (_currentTheme.TitleText.Font.Style != value)
                {
                    _currentTheme.TitleText.Font.Style = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public OnlyVFontWeight TitleTextFontWeight
        {
            get => _currentTheme.TitleText.Font.Weight;
            set
            {
                if (_currentTheme.TitleText.Font.Weight != value)
                {
                    _currentTheme.TitleText.Font.Weight = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public Color? TitleTextColour
        {
            get => ConvertFromString(_currentTheme.TitleText.Font.Colour, _defaultTextColor);
            set
            {
                if (value == null)
                {
                    value = _defaultTextColor;
                }

                if (ConvertFromString(_currentTheme.TitleText.Font.Colour, _defaultTextColor) != value)
                {
                    _currentTheme.TitleText.Font.Colour = value.Value.ToString();
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public OnlyVHorizontalTextAlignment TitleTextHorizontalAlignment
        {
            get => _currentTheme.TitleText.HorizontalAlignment;
            set
            {
                if (_currentTheme.TitleText.HorizontalAlignment != value)
                {
                    _currentTheme.TitleText.HorizontalAlignment = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public OnlyVTitlePosition TitlePosition
        {
            get => _currentTheme.TitleText.Position;
            set
            {
                if (_currentTheme.TitleText.Position != value)
                {
                    _currentTheme.TitleText.Position = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }
        
        public bool TitleTextDropShadow
        {
            get => _currentTheme.TitleText.DropShadow.Show;
            set
            {
                if (_currentTheme.TitleText.DropShadow.Show != value)
                {
                    _currentTheme.TitleText.DropShadow.Show = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public Color? TitleDropShadowColour
        {
            get => ConvertFromString(_currentTheme.TitleText.DropShadow.Colour, _defaultDropShadowColor);
            set
            {
                if (value == null)
                {
                    value = _defaultDropShadowColor;
                }

                if (ConvertFromString(_currentTheme.TitleText.DropShadow.Colour, _defaultDropShadowColor) != value)
                {
                    _currentTheme.TitleText.DropShadow.Colour = value.Value.ToString();
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public double TitleDropShadowOpacity
        {
            get => _currentTheme.TitleText.DropShadow.Opacity;
            set
            {
                SetOpacityProperty(nameof(TitleDropShadowOpacity), _currentTheme.TitleText.DropShadow.Opacity, value, v => _currentTheme.TitleText.DropShadow.Opacity = v);
            }
        }

        public double TitleDropShadowBlurRadius
        {
            get => _currentTheme.TitleText.DropShadow.BlurRadius;
            set
            {
                if (Math.Abs(_currentTheme.TitleText.DropShadow.BlurRadius - value) > BlurRadiusTolerance && 
                    IsValidBlurRadius(value))
                {
                    _currentTheme.TitleText.DropShadow.BlurRadius = value;

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

        public double TitleDropShadowDepth
        {
            get => _currentTheme.TitleText.DropShadow.Depth;
            set
            {
                if (Math.Abs(_currentTheme.TitleText.DropShadow.Depth - value) > ShadowDepthTolerance && 
                    IsValidShadowDepth(value))
                {
                    _currentTheme.TitleText.DropShadow.Depth = value;

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

        public bool ShowVerseNumbers
        {
            get => _currentTheme.VerseNumbers.Show;
            set
            {
                if (_currentTheme.VerseNumbers.Show != value)
                {
                    _currentTheme.VerseNumbers.Show = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public OnlyVFontStyle VerseFontStyle
        {
            get => _currentTheme.VerseNumbers.Style;
            set
            {
                if (_currentTheme.VerseNumbers.Style != value)
                {
                    _currentTheme.VerseNumbers.Style = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public OnlyVFontWeight VerseFontWeight
        {
            get => _currentTheme.VerseNumbers.Weight;
            set
            {
                if (_currentTheme.VerseNumbers.Weight != value)
                {
                    _currentTheme.VerseNumbers.Weight = value;
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public Color? VerseColour
        {
            get => ConvertFromString(_currentTheme.VerseNumbers.Colour, _defaultTextColor);
            set
            {
                if (value == null)
                {
                    value = _defaultTextColor;
                }

                if (ConvertFromString(_currentTheme.VerseNumbers.Colour, _defaultTextColor) != value)
                {
                    _currentTheme.VerseNumbers.Colour = value.Value.ToString();
                    RaisePropertyChanged();
                    UpdateImage();
                }
            }
        }

        public double VerseOpacity
        {
            get => _currentTheme.VerseNumbers.Opacity;
            set
            {
                SetOpacityProperty(nameof(VerseOpacity), _currentTheme.VerseNumbers.Opacity, value, v => _currentTheme.VerseNumbers.Opacity = v);
            }
        }

        public RelayCommand NewFileCommand { get; set; }

        public RelayCommand OpenFileCommand { get; set; }

        public RelayCommand SaveFileCommand { get; set; }

        public RelayCommand SaveAsFileCommand { get; set; }

        public RelayCommand ClearBackgroundImageCommand { get; set; }

        public RelayCommand ClosedCommand { get; set; }

        public RelayCommand ClosingCommand { get; set; }

        public RelayCommand CancelClosingCommand { get; set; }

        private void UpdateTextSamples()
        {
            var originalSampleId = CurrentSampleTextId;
            CurrentSampleTextId = 0;

            TextSamples.Clear();
            
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
            return reader.GenerateVerseTitle(
                sample.BookNumber, sample.ChapterAndVerses, false, true);
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

                var images = _imageService.Generate(CurrentEpubFilePath, sample.BookNumber, sample.ChapterAndVerses)?.ToArray();
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

            _imageService.BackgroundImageOpacity = BackgroundImageOpacity;

            _imageService.BackgroundColor = BackgroundColour ?? _defaultBackgroundColor;

            // body text...
            _imageService.MainFont.FontFamily = new FontFamily(BodyTextFontFamilyName);
            _imageService.MainFont.FontSize = BodyTextSize;
            _imageService.MainFont.FontStyle = BodyTextFontStyle.AsWindowsFontStyle();
            _imageService.MainFont.FontWeight = BodyTextFontWeight.AsWindowsFontWeight();
            _imageService.MainFont.FontColor = BodyTextColour ?? _defaultTextColor;
            _imageService.MainFont.Opacity = BodyTextOpacity;
            _imageService.HorzAlignment = BodyTextHorizontalAlignment.AsWindowsTextAlignment();
            _imageService.LineSpacing = LineSpacing;
            _imageService.BodyDropShadow = BodyTextDropShadow;
            _imageService.BodyDropShadowColor = BodyDropShadowColour ?? _defaultDropShadowColor;
            _imageService.BodyDropShadowOpacity = BodyDropShadowOpacity;
            _imageService.BodyDropShadowBlurRadius = BodyDropShadowBlurRadius;
            _imageService.BodyDropShadowDepth = BodyDropShadowDepth;
            _imageService.BodyVerticalAlignment = BodyVerticalAlignment;

            // title text...
            _imageService.TitleFont.FontFamily = new FontFamily(TitleTextFontFamilyName);
            _imageService.TitleFont.FontSize = TitleTextSize;
            _imageService.TitleFont.FontStyle = TitleTextFontStyle.AsWindowsFontStyle();
            _imageService.TitleFont.FontWeight = TitleTextFontWeight.AsWindowsFontWeight();
            _imageService.TitleFont.FontColor = TitleTextColour ?? _defaultTextColor;
            _imageService.TitleFont.Opacity = TitleTextOpacity;
            _imageService.TitleHorzAlignment = TitleTextHorizontalAlignment.AsWindowsTextAlignment();
            _imageService.TitlePosition = TitlePosition;
            _imageService.TitleDropShadow = TitleTextDropShadow;
            _imageService.TitleDropShadowColor = TitleDropShadowColour ?? _defaultDropShadowColor;
            _imageService.TitleDropShadowOpacity = TitleDropShadowOpacity;
            _imageService.TitleDropShadowBlurRadius = TitleDropShadowBlurRadius;
            _imageService.TitleDropShadowDepth = TitleDropShadowDepth;

            // verse numbers...
            _imageService.ShowVerseNumbers = ShowVerseNumbers;
            _imageService.VerseFont.FontFamily = _imageService.MainFont.FontFamily;
            _imageService.VerseFont.FontStyle = VerseFontStyle.AsWindowsFontStyle();
            _imageService.VerseFont.FontWeight = VerseFontWeight.AsWindowsFontWeight();
            _imageService.VerseFont.FontColor = VerseColour ?? _defaultTextColor;
            _imageService.VerseFont.Opacity = VerseOpacity;

            // misc (hard-coded here)...
            _imageService.UseTildeParaSeparator = false;
            _imageService.TrimQuotes = false;
            _imageService.TrimPunctuation = false;
            _imageService.AllowAutoFit = false;
            _imageService.FlowDirection = FlowDirection.LeftToRight;
            _imageService.CultureInfo = CultureInfo.CurrentUICulture;
        }

        private void InitCommands()
        {
            NewFileCommand = new RelayCommand(NewFile, CanExecuteNewFile);
            OpenFileCommand = new RelayCommand(OpenFile, CanExecuteOpenFile);
            SaveFileCommand = new RelayCommand(SaveFile, CanExecuteSaveFile);
            SaveAsFileCommand = new RelayCommand(SaveAsFile, CanExecuteSaveAsFile);
            ClearBackgroundImageCommand = new RelayCommand(ClearBackgroundImage, CanExecuteClearBackgroundImage);

            ClosedCommand = new RelayCommand(ExecuteClosed);
            ClosingCommand = new RelayCommand(ExecuteClosing, CanExecuteClosing);
            CancelClosingCommand = new RelayCommand(ExecuteCancelClosing);
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
            if (string.IsNullOrEmpty(_currentThemePath))
            {
                return false;
            }

            return true;
        }

        private void SaveAsFile()
        {
            using (var d = new CommonSaveFileDialog())
            {
                d.OverwritePrompt = true;
                d.AlwaysAppendDefaultExtension = true;
                d.IsExpandedMode = true;
                d.DefaultDirectory = _defaultFileSaveFolder ?? FileUtils.GetPrivateThemeFolder();
                d.DefaultExtension = ThemeFile.ThemeFileExtension;
                d.Filters.Add(new CommonFileDialogFilter(Properties.Resources.THEME_FILE, $"*{ThemeFile.ThemeFileExtension}"));
                d.Title = Properties.Resources.SAVE_THEME_TITLE;

                var rv = d.ShowDialog();
                if (rv == CommonFileDialogResult.Ok)
                {
                    _defaultFileSaveFolder = Path.GetDirectoryName(d.FileName);
                    var themePath = d.FileName;

                    var f = new ThemeFile();
                    f.Create(themePath, _currentTheme, _backgroundImage, overwrite: true);

                    CurrentThemePath = themePath;
                    SaveSignature();
                }
            }
        }

        private bool CanExecuteSaveFile()
        {
            if (string.IsNullOrEmpty(_currentThemePath))
            {
                return true;
            }

            return IsDirty;
        }

        private void SaveFile()
        {
            if (string.IsNullOrEmpty(_currentThemePath))
            {
                SaveAsFile();
            }
            else
            {
                ThemeFile f = new ThemeFile();
                f.Create(_currentThemePath, _currentTheme, _backgroundImage, overwrite: true);
                SaveSignature();
            }
        }

        private bool CanExecuteOpenFile()
        {
            return true;
        }

        private async void OpenFile()
        {
            if (IsDirty)
            {
                var result = await _dialogService.ShouldSaveDirtyDataAsync().ConfigureAwait(true);
                if (result == true)
                {
                    SaveFile();
                }
                else if (result == null)
                {
                    return;
                }
            }

            using (var d = new CommonOpenFileDialog())
            {
                d.DefaultDirectory = _defaultFileOpenFolder ?? FileUtils.GetPrivateThemeFolder();
                d.DefaultExtension = ThemeFile.ThemeFileExtension;
                d.Filters.Add(new CommonFileDialogFilter(Properties.Resources.THEME_FILE, $"*{ThemeFile.ThemeFileExtension}"));
                d.Title = Properties.Resources.OPEN_THEME_TITLE;

                var rv = d.ShowDialog();
                if (rv == CommonFileDialogResult.Ok)
                {
                    _defaultFileOpenFolder = Path.GetDirectoryName(d.FileName);
                    
                    var f = new ThemeFile();
                    var t = f.Read(d.FileName);
                    if (t != null)
                    {
                        CurrentTheme = t.Theme;
                        CurrentThemePath = d.FileName;

                        BackgroundImage = t.BackgroundImage == null 
                            ? null 
                            : ConvertImageSourceToBitmapImage(t.BackgroundImage);

                        UpdateImage();
                        SaveSignature();
                    }
                }
            }
        }

        private BitmapImage ConvertImageSourceToBitmapImage(ImageSource backgroundImage)
        {
            var bitmapSource = backgroundImage as BitmapSource;
            if (bitmapSource == null)
            {
                return null;
            }
            
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            var stream = new MemoryStream();
            
            encoder.Save(stream);
            var result = new BitmapImage();
            
            result.BeginInit();
            stream.Seek(0, SeekOrigin.Begin);
            result.StreamSource = stream;
            result.EndInit();

            return result;
        }

        private bool CanExecuteNewFile()
        {
            return true;
        }

        private async void NewFile()
        {
            if (IsDirty)
            {
                var result = await _dialogService.ShouldSaveDirtyDataAsync().ConfigureAwait(true);
                if (result == true)
                {
                    SaveFile();
                }
                else if (result == null)
                {
                    return;
                }

                CurrentThemePath = null;
                CurrentTheme = new OnlyVTheme();
                BackgroundImage = null;

                SaveSignature();

                UpdateImage();
            }
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
                ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase) ||
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

        private bool IsValidBlurRadius(double value)
        {
            return value >= 0.0 && value <= MaxBlurRadiusValue;
        }

        private bool IsValidShadowDepth(double value)
        {
            return value >= 0.0 && value <= MaxShadowDepthValue;
        }

        private void SetOpacityProperty(
            string propertyName,
            double currentValue, 
            double newValue, 
            Action<double> setter)
        {
            if (Math.Abs(currentValue - newValue) > OpacityTolerance && 
                IsValidOpacity(newValue))
            {
                setter(newValue);

                _singleExecAction.Execute(() =>
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        RaisePropertyChanged(propertyName);
                        UpdateImage();
                    });
                });
            }
        }

        private SystemFont[] GetSystemFonts()
        {
            var result = new List<SystemFont>();

            foreach (var f in Fonts.SystemFontFamilies)
            {
                result.Add(new SystemFont
                {
                    FamilyName = f.Source
                });
            }

            return result.ToArray();
        }

        private string CreateThemeSignature()
        {
            var signature = JsonConvert.SerializeObject(_currentTheme);
            return string.Concat(signature, _backgroundImage?.GetHashCode());
        }

        private void SaveSignature()
        {
            _lastSavedThemeSignature = CreateThemeSignature();
            RaisePropertyChanged(nameof(IsDirty));

            CommandManager.InvalidateRequerySuggested();
        }

        private void ExecuteClosed()
        {
        }
        
        private void ExecuteClosing()
        {
        }

        private bool CanExecuteClosing()
        {
            return !_dialogService.IsDialogVisible() && !IsDirty;
        }

        private async void ExecuteCancelClosing()
        {
            if (_dialogService.IsDialogVisible())
            {
                return;
            }

            var rv = await _dialogService.ShouldSaveDirtyDataAsync().ConfigureAwait(true);
            if (rv == true)
            {
                SaveFile();
            }

            if (rv != null)
            {
                // User answered "No". Make the data not dirty by saving current sig...
                SaveSignature();
                Messenger.Default.Send(new CloseAppMessage());
            }
        }
    }
}