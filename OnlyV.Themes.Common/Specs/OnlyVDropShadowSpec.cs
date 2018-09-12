namespace OnlyV.Themes.Common.Specs
{
    // ReSharper disable MemberCanBePrivate.Global
    using System.Drawing;

    public class OnlyVDropShadowSpec
    {
        public OnlyVDropShadowSpec()
        {
            Show = true;
            Colour = ColorTranslator.ToHtml(Color.DarkGray);
            Opacity = 0.8;
            BlurRadius = 10;
            Depth = 10;
        }

        public bool Show { get; set; }

        public string Colour { get; set; }

        public double Opacity { get; set; }

        public double BlurRadius { get; set; }

        public double Depth { get; set; }
    }
}
