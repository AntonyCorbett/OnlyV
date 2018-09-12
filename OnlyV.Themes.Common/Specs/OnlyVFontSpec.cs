namespace OnlyV.Themes.Common.Specs
{
    // ReSharper disable MemberCanBePrivate.Global
    using System.Drawing;

    public class OnlyVFontSpec
    {
        public OnlyVFontSpec()
        {
            Family = "Georgia";
            Size = 74.0;
            Style = OnlyVFontStyle.Normal;
            Weight = OnlyVFontWeight.Normal;
            Colour = ColorTranslator.ToHtml(Color.Wheat);
            Opacity = 1.0;
        }

        public string Family { get; set; }

        public double Size { get; set; }

        public OnlyVFontStyle Style { get; set; }

        public OnlyVFontWeight Weight { get; set; }

        public string Colour { get; set; }

        public double Opacity { get; set; }
    }
}
