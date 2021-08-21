using System.Drawing;

namespace OnlyV.Themes.Common.Specs
{
    // ReSharper disable MemberCanBePrivate.Global
    public class OnlyVDropShadowSpec
    {
        public OnlyVDropShadowSpec()
        {
            Show = true;
            Colour = ColorTranslator.ToHtml(Color.Black);
            Opacity = 0.7;
            BlurRadius = 20;
            Depth = 10;
        }

        public bool Show { get; set; }

        public string Colour { get; set; }

        public double Opacity { get; set; }

        public double BlurRadius { get; set; }

        public double Depth { get; set; }
    }
}
