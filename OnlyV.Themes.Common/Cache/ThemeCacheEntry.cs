using System.Windows.Media;

namespace OnlyV.Themes.Common.Cache
{
    public class ThemeCacheEntry
    {
        public string ThemePath { get; set; }

        public OnlyVTheme Theme { get; set; }

        public ImageSource BackgroundImage { get; set; }
    }
}
