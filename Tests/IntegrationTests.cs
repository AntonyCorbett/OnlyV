namespace Tests
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using OnlyV.ImageCreation;
    using OnlyV.Themes.Common;
    using OnlyV.Themes.Common.FileHandling;
    using OnlyV.VerseExtraction.Models;
    using OnlyV.VerseExtraction.Parser;

    [TestClass]
    public class IntegrationTests
    {
        private const string EpubPath = "nwt_E.epub";

        [TestMethod]
        public void TestBibleEpubParser()
        {
            var parser = new BibleEpubParser(EpubPath);
            var bookData = parser.ExtractBookData();
            Assert.IsNotNull(bookData);

            // Gen 1:1
            var formattingOptions = new FormattingOptions();
            var s = parser.ExtractVerseText(1, 1, 1, formattingOptions);
            Assert.IsNotNull(s);

            var s2 = parser.ExtractVersesText(1, "1:1-3", formattingOptions);
            Assert.IsNotNull(s2);
        }

        [TestMethod]
        public void TestImageCreation()
        {
            BibleTextImage image = new BibleTextImage();
            var images = image.Generate(EpubPath, 1, "3:15");
        }

        [TestMethod]
        public void TestThemeCreation()
        {
            CreateThemeFile("OnlyV Bible", "bible01.png", "#fffcf9", "#2274a5", "#632b30", "Georgia", 200);
            CreateThemeFile("OnlyV Blue", "blue01.png", "#28598B", "#fcf7f8", "#e6e8e6", "Cambria");
            CreateThemeFile("OnlyV Dark Blue", "blue02.png", "#181770", "#fcf7f8", "#e6e8e6", "Calibri");
            CreateThemeFile("OnlyV Chalkboard", "chalkboard.png", "#3C3C3C", "#fff8f0", "#fff8f0", "Constantia");
            CreateThemeFile("OnlyV Clouds", "clouds01.png", "#DBE0E4", "#101935", "#003459", "Trebuchet");
            CreateThemeFile("OnlyV Light Marble", "marble02.png", "#E8CAAD", "#19535f", "#3b252c", "Trebuchet");
            CreateThemeFile("OnlyV Paper", "paper01.png", "#D4B994", "#3b252c", "#8f6593", "Tahoma");
            CreateThemeFile("OnlyV Rough Marble", "marble01.png", "#C5C6C1", "#ffffff", "#ffffff");
            CreateThemeFile("OnlyV Water", "water01.png", "#8B94A8", "#141301", "#1a1d1a", "Cambria");
        }

        private void CreateThemeFile(
            string descriptiveName, 
            string imagePath,
            string backgroundColor,
            string mainTextColor,
            string secondaryTextColor,
            string fontFamily = "Georgia",
            int bottomMargin = 100)
        {
            var file = new ThemeFile();

            var currentFolder = Environment.CurrentDirectory;
            var themePath = Path.Combine(currentFolder, $"{descriptiveName}.onlyv");

            var theme = new OnlyVTheme();
            theme.Background.Colour = backgroundColor;

            theme.BodyText.Font.Family = fontFamily;
            theme.BodyText.Font.Colour = mainTextColor;
            theme.BodyText.DropShadow.Opacity = 0.3;
            theme.BodyText.DropShadow.Depth = 7;
            theme.BodyText.DropShadow.BlurRadius = 15;

            theme.TitleText.Font.Family = fontFamily;
            theme.TitleText.Font.Colour = secondaryTextColor;
            theme.TitleText.DropShadow.Opacity = 0.3;
            theme.TitleText.DropShadow.Depth = 7;
            theme.TitleText.DropShadow.BlurRadius = 15;

            theme.VerseNumbers.Colour = secondaryTextColor;

            theme.Dimensions.BottomMargin = bottomMargin;

            file.Create(themePath, theme, imagePath, overwrite: true);

            var result = file.Read(themePath);
            Assert.IsNotNull(result);

            var s1 = JsonConvert.SerializeObject(result.Theme);
            var s2 = JsonConvert.SerializeObject(theme);
            Assert.AreEqual(s1, s2);
        }
    }
}
