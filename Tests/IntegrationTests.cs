using System;
using System.IO;
using Newtonsoft.Json;
using OnlyV.ImageCreation;
using OnlyV.Themes.Common;
using OnlyV.Themes.Common.FileHandling;

namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var file = new ThemeFile();

            var currentFolder = Environment.CurrentDirectory;
            var themePath = Path.Combine(currentFolder, "myTheme.onlyv");

            var theme = new OnlyVTheme();
            theme.Background.UseImage = true;
            theme.Background.Colour = "#fffcf9";

            theme.BodyText.Font.Colour = "#2274a5";
            theme.BodyText.DropShadow.Opacity = 0.3;
            theme.BodyText.DropShadow.Depth = 7;
            theme.BodyText.DropShadow.BlurRadius = 15;

            theme.TitleText.Font.Colour = "#632b30";
            theme.TitleText.DropShadow.Opacity = 0.3;
            theme.TitleText.DropShadow.Depth = 7;
            theme.TitleText.DropShadow.BlurRadius = 15;

            theme.VerseNumbers.Colour = "#1a1423";
            
            theme.Dimensions.BottomMargin = 200;

            file.Create(themePath, theme, "Bible01.png", overwrite: true);

            var result = file.Read(themePath);
            Assert.IsNotNull(result);

            var s1 = JsonConvert.SerializeObject(result.Theme);
            var s2 = JsonConvert.SerializeObject(theme);
            Assert.AreEqual(s1, s2);
        }
    }
}
