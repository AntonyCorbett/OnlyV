using System;
using System.Globalization;
using System.Windows.Data;
using OnlyV.Themes.Common.Specs;

namespace OnlyV.Themes.Common.Converters
{
    public class OnlyVLineSpacingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OnlyVLineSpacing spacing)
            {
                switch (spacing)
                {
                    case OnlyVLineSpacing.VerySmall:
                        return Properties.Resources.LINE_SPACING_VERY_SMALL;
                    case OnlyVLineSpacing.Small:
                        return Properties.Resources.LINE_SPACING_SMALL;
                    case OnlyVLineSpacing.Normal:
                        return Properties.Resources.LINE_SPACING_NORMAL;
                    case OnlyVLineSpacing.Large:
                        return Properties.Resources.LINE_SPACING_LARGE;
                    case OnlyVLineSpacing.VeryLarge:
                        return Properties.Resources.LINE_SPACING_VERY_LARGE;
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
