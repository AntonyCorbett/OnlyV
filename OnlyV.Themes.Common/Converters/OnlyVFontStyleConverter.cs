namespace OnlyV.Themes.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using OnlyV.Themes.Common.Specs;

    public class OnlyVFontStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OnlyVFontStyle style)
            {
                switch (style)
                {
                    case OnlyVFontStyle.Normal:
                        return Properties.Resources.FONT_STYLE_NORMAL;
                    case OnlyVFontStyle.Italic:
                        return Properties.Resources.FONT_STYLE_ITALIC;
                    case OnlyVFontStyle.Oblique:
                        return Properties.Resources.FONT_STYLE_OBLIQUE;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
