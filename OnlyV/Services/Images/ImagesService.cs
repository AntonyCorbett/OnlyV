namespace OnlyV.Services.Images
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Extensions;
    using ImageCreation;
    using Themes.Common;
    using Themes.Common.FileHandling;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ImagesService : IImagesService
    {
        private BitmapSource[] _images;

        public int ImageCount => _images?.Length ?? 0;

        public void Init(string epubPath, string themePath, int bookNumber, string chapterAndVerses)
        {
            var bibleTextImage = new BibleTextImage();

            ApplyFormatting(bibleTextImage, themePath);

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

        private void ApplyFormatting(BibleTextImage bibleTextImage, string themePath)
        {
            var file = new ThemeFile();
            var cacheEntry = file.Read(themePath);

            var theme = cacheEntry == null
                ? new OnlyVTheme()
                : cacheEntry.Theme;

            var backgroundImage = cacheEntry?.BackgroundImage;

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
            bibleTextImage.BackgroundImageSource = theme.Background.UseImage ? backgroundImage : null;

            // formatting...
            bibleTextImage.AllowAutoFit = theme.Formatting.AutoFit;
            bibleTextImage.ShowBreakInVerses = theme.Formatting.ShowVerseBreaks;
            bibleTextImage.UseTildeParaSeparator = theme.Formatting.UseTildeMarker;
            bibleTextImage.TrimPunctuation = theme.Formatting.TrimPunctuation;
            bibleTextImage.TrimQuotes = theme.Formatting.TrimQuotes;

            // Continuation arrow...
            bibleTextImage.ShowContinuationArrow = theme.ContinuationArrow.Show;
            bibleTextImage.ContinuationArrowColor = ConvertFromString(theme.ContinuationArrow.Colour, Colors.Azure);
            bibleTextImage.ContinuationArrowOpacity = theme.ContinuationArrow.Opacity;

            // body text...
            bibleTextImage.MainFont.FontFamily = new FontFamily(theme.BodyText.Font.Family);
            bibleTextImage.MainFont.FontSize = theme.BodyText.Font.Size;
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
            bibleTextImage.TitleFont.FontSize = theme.TitleText.Font.Size;
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

            bibleTextImage.ShowVerseNumbers = theme.VerseNumbers.Show;
            bibleTextImage.VerseFont.FontFamily = new FontFamily(verseNosFontFamily);
            bibleTextImage.VerseFont.FontColor = ConvertFromString(theme.VerseNumbers.Colour, Colors.White);
            bibleTextImage.VerseFont.FontStyle = theme.VerseNumbers.Style.AsWindowsFontStyle();
            bibleTextImage.VerseFont.FontWeight = theme.VerseNumbers.Weight.AsWindowsFontWeight();
            bibleTextImage.VerseFont.Opacity = theme.VerseNumbers.Opacity;
        }
    }
}
