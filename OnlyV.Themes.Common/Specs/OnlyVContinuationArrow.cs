namespace OnlyV.Themes.Common.Specs
{
    // ReSharper disable MemberCanBePrivate.Global
    using System.Drawing;
    
    public class OnlyVContinuationArrow
    {
        public OnlyVContinuationArrow()
        {
            Colour = ColorTranslator.ToHtml(Color.AliceBlue);
            Opacity = 0.7;
        }

        public bool Show { get; set; }

        public string Colour { get; set; }
        
        public double Opacity { get; set; }
    }
}
