namespace OnlyV.Themes.Common.Cache
{
    using System.Windows.Media;

    public class ThemeCacheEntry
    {
        public string ThemePath { get; set; }

        public OnlyVTheme Theme { get; set; }

        public ImageSource BackgroundImage { get; set; }
    }
}
