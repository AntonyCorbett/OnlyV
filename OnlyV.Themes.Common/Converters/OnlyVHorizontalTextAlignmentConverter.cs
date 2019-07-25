namespace OnlyV.Themes.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using OnlyV.Themes.Common.Specs;

    public class OnlyVHorizontalTextAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OnlyVHorizontalTextAlignment alignment)
            {
                switch (alignment)
                {
                    case OnlyVHorizontalTextAlignment.Centre:
                        return Properties.Resources.HORZ_TEXT_ALIGNMENT_CENTRE;

                    case OnlyVHorizontalTextAlignment.Left:
                        return Properties.Resources.HORZ_TEXT_ALIGNMENT_LEFT;

                    case OnlyVHorizontalTextAlignment.Right:
                        return Properties.Resources.HORZ_TEXT_ALIGNMENT_RIGHT;
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
