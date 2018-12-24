namespace OnlyV.Services.Images
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Helpers;
    using ImageCreation;
    using OnlyV.Services.VerseEditor;
    using OnlyV.Themes.Common.Extensions;
    using OnlyV.Themes.Common.Services.UI;
    using Options;
    using Themes.Common;
    using Themes.Common.FileHandling;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ImagesService : IImagesService
    {
        private readonly IOptionsService _optionsService;
        private readonly IUserInterfaceService _userInterfaceService;
        private readonly IVerseEditorService _verseEditorService;

        private BitmapSource[] _images;
        private int _currentBookNumber;
        private string _currentChapterAndVerses;
        
        public ImagesService(
            IOptionsService optionsService,
            IUserInterfaceService userInterfaceService,
            IVerseEditorService verseEditorService)
        {
            _optionsService = optionsService;
            _userInterfaceService = userInterfaceService;
            _verseEditorService = verseEditorService;
        }

        public int ImageCount => _images?.Length ?? 0;

        public bool VerseTextIsModified { get; private set; }

        public void Init(int bookNumber, string chapterAndVerses)
        {
            _currentBookNumber = bookNumber;
            _currentChapterAndVerses = chapterAndVerses;

            Refresh();
        }

        public void Refresh()
        {
            if (_currentBookNumber > 0 && !string.IsNullOrEmpty(_currentChapterAndVerses))
            {
                VerseTextIsModified = false;

                using (_userInterfaceService.GetBusy())
                {
                    var bibleTextImage = new BibleTextImage();
                    bibleTextImage.VerseFetchEvent += HandleVerseFetchEvent;

                    ApplyFormatting(bibleTextImage, _optionsService.ThemePath);

                    _images = _optionsService.EpubPath != null
                        ? bibleTextImage.Generate(
                                _optionsService.EpubPath, 
                                _currentBookNumber,
                                _currentChapterAndVerses).ToArray()
                        : null;
                }
            }
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

        private void ApplyFormatting(BibleTextImage bibleTextImage, string themePath)
        {
            var file = new ThemeFile();
            var cacheEntry = file.Read(themePath);

            OnlyVTheme theme;
            ImageSource backgroundImage;
            if (cacheEntry == null)
            {
                // must use default...
                theme = new OnlyVTheme();
                backgroundImage = BitmapHelper.ConvertBitmap(Properties.Resources.Blue);
            }
            else
            {
                theme = cacheEntry.Theme;
                backgroundImage = cacheEntry.BackgroundImage;
            }
            
            ApplyFormatting(bibleTextImage, theme, backgroundImage);
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

        private void ApplyFormatting(
            BibleTextImage bibleTextImage, 
            OnlyVTheme theme, 
            ImageSource backgroundImage)
        {
            // dimensions...
            bibleTextImage.Width = theme.Dimensions.ImageWidth;
            bibleTextImage.Height = theme.Dimensions.ImageHeight;
            bibleTextImage.LeftMargin = theme.Dimensions.LeftMargin;
            bibleTextImage.TopMargin = theme.Dimensions.TopMargin;
            bibleTextImage.RightMargin = theme.Dimensions.RightMargin;
            bibleTextImage.BottomMargin = theme.Dimensions.BottomMargin;

            // background...
            bibleTextImage.BackgroundColor = ConvertFromString(theme.Background.Colour, Colors.Blue);
            bibleTextImage.BackgroundImageSource = _optionsService.UseBackgroundImage
                ? backgroundImage
                : null;
            bibleTextImage.BackgroundImageOpacity = theme.Background.ImageOpacity;

            // formatting...
            bibleTextImage.AllowAutoFit = _optionsService.AutoFit;
            bibleTextImage.ShowBreakInVerses = _optionsService.ShowVerseBreaks;
            bibleTextImage.UseTildeParaSeparator = _optionsService.UseTildeMarker;
            bibleTextImage.TrimPunctuation = _optionsService.TrimPunctuation;
            bibleTextImage.TrimQuotes = _optionsService.TrimQuotes;

            // body text...
            bibleTextImage.MainFont.FontFamily = new FontFamily(theme.BodyText.Font.Family);
            bibleTextImage.MainFont.FontSize = AdaptToScaling(theme.BodyText.Font.Size);
            bibleTextImage.MainFont.FontColor = ConvertFromString(theme.BodyText.Font.Colour, Colors.White);
            bibleTextImage.MainFont.FontStyle = theme.BodyText.Font.Style.AsWindowsFontStyle();
            bibleTextImage.MainFont.FontWeight = theme.BodyText.Font.Weight.AsWindowsFontWeight();
            bibleTextImage.MainFont.Opacity = theme.BodyText.Font.Opacity;
            bibleTextImage.HorzAlignment = theme.BodyText.HorizontalAlignment.AsWindowsTextAlignment();
            bibleTextImage.LineSpacing = theme.BodyText.LineSpacing;
            bibleTextImage.BodyDropShadow = theme.BodyText.DropShadow.Show;
            bibleTextImage.BodyDropShadowBlurRadius = theme.BodyText.DropShadow.BlurRadius;
            bibleTextImage.BodyDropShadowColor = ConvertFromString(theme.BodyText.DropShadow.Colour, Colors.Black);
            bibleTextImage.BodyDropShadowDepth = theme.BodyText.DropShadow.Depth;
            bibleTextImage.BodyDropShadowOpacity = theme.BodyText.DropShadow.Opacity;

            // title text...
            bibleTextImage.TitleFont.FontFamily = new FontFamily(theme.TitleText.Font.Family);
            bibleTextImage.TitleFont.FontSize = AdaptToScaling(theme.TitleText.Font.Size);
            bibleTextImage.TitleFont.FontColor = ConvertFromString(theme.TitleText.Font.Colour, Colors.White);
            bibleTextImage.TitleFont.FontStyle = theme.TitleText.Font.Style.AsWindowsFontStyle();
            bibleTextImage.TitleFont.FontWeight = theme.TitleText.Font.Weight.AsWindowsFontWeight();
            bibleTextImage.TitleFont.Opacity = theme.TitleText.Font.Opacity;
            bibleTextImage.TitleHorzAlignment = theme.TitleText.HorizontalAlignment.AsWindowsTextAlignment();
            bibleTextImage.TitlePosition = theme.TitleText.Position;
            bibleTextImage.TitleDropShadow = theme.TitleText.DropShadow.Show;
            bibleTextImage.TitleDropShadowBlurRadius = theme.TitleText.DropShadow.BlurRadius;
            bibleTextImage.TitleDropShadowColor = ConvertFromString(theme.TitleText.DropShadow.Colour, Colors.Black);
            bibleTextImage.TitleDropShadowDepth = theme.TitleText.DropShadow.Depth;
            bibleTextImage.TitleDropShadowOpacity = theme.TitleText.DropShadow.Opacity;

            // verse nos...
            // font family same as body font
            var verseNosFontFamily = theme.BodyText.Font.Family;

            bibleTextImage.ShowVerseNumbers = _optionsService.ShowVerseNos;
            bibleTextImage.VerseFont.FontFamily = new FontFamily(verseNosFontFamily);
            bibleTextImage.VerseFont.FontColor = ConvertFromString(theme.VerseNumbers.Colour, Colors.White);
            bibleTextImage.VerseFont.FontStyle = theme.VerseNumbers.Style.AsWindowsFontStyle();
            bibleTextImage.VerseFont.FontWeight = theme.VerseNumbers.Weight.AsWindowsFontWeight();
            bibleTextImage.VerseFont.Opacity = theme.VerseNumbers.Opacity;
        }

        private double AdaptToScaling(double fontSize)
        {
            return (fontSize * _optionsService.TextScalingPercentage) / 100;
        }

        private void HandleVerseFetchEvent(object sender, VerseExtraction.Models.VerseAndText e)
        {
            var modifiedVerse = _verseEditorService.Get(_optionsService.EpubPath, e.BookNumber, e.ChapterNumber, e.VerseNumber);
            if (!string.IsNullOrEmpty(modifiedVerse))
            {
                VerseTextIsModified = true;
                e.Text = modifiedVerse.Trim();
            }
        }
    }
}
