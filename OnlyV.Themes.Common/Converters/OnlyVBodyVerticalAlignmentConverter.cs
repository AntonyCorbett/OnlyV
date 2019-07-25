namespace OnlyV.Themes.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using OnlyV.Themes.Common.Specs;

    public class OnlyVBodyVerticalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OnlyVBodyVerticalAlignment alignment)
            {
                switch (alignment)
                {
                    case OnlyVBodyVerticalAlignment.Middle:
                        return Properties.Resources.BODY_VERTICAL_ALIGNMENT_MIDDLE;

                    case OnlyVBodyVerticalAlignment.Top:
                        return Properties.Resources.BODY_VERTICAL_ALIGNMENT_TOP;
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
