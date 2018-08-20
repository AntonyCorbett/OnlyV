namespace OnlyV.ImageCreation.Utils
{
    using System.Windows;
    using System.Windows.Media;

    public class FontDefinition
    {
        private Color _fontColor;
        private double _opacity;
        private SolidColorBrush _brush;

        public FontDefinition()
        {
            Opacity = 1.0;
        }

        public double FontSize { get; set; }

        public FontStyle FontStyle { get; set; }

        public FontWeight FontWeight { get; set; }

        public FontFamily FontFamily { get; set; }

        public Color FontColor
        {
            get => _fontColor;
            set
            {
                if (_fontColor != value)
                {
                    _fontColor = value;
                    _brush = null;
                }
            }
        }

        public double Opacity
        {
            get => _opacity;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_opacity != value)
                {
                    _opacity = value;
                    _brush = null;
                }
            }
        }

        public Typeface GetTypeface()
        {
            return new Typeface(FontFamily, FontStyle, FontWeight, FontStretches.Normal);
        }

        public Brush GetBrush()
        {
            return _brush ?? (_brush = new SolidColorBrush(FontColor) { Opacity = Opacity });
        }
    }
}
