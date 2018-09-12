namespace OnlyV.Themes.Common.Specs
{
    // ReSharper disable MemberCanBePrivate.Global
    public class OnlyVVerseNumberSpec
    {
        public OnlyVVerseNumberSpec()
        {
            Show = true;
            Style = OnlyVFontStyle.Normal;
            Weight = OnlyVFontWeight.Light;
            Colour = "#7a5c61";
            Opacity = 1.0;
        }

        public bool Show { get; set; }

        public OnlyVFontStyle Style { get; set; }

        public OnlyVFontWeight Weight { get; set; }

        public string Colour { get; set; }

        public double Opacity { get; set; }
    }
}
