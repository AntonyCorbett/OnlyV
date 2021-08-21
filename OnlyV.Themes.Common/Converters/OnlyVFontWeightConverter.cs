using System;
using System.Globalization;
using System.Windows.Data;
using OnlyV.Themes.Common.Specs;

namespace OnlyV.Themes.Common.Converters
{
    public class OnlyVFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OnlyVFontWeight weight)
            {
                switch (weight)
                {
                    case OnlyVFontWeight.Light:
                        return Properties.Resources.FONT_WEIGHT_LIGHT;
                    case OnlyVFontWeight.Normal:
                        return Properties.Resources.FONT_WEIGHT_NORMAL;
                    case OnlyVFontWeight.SemiBold:
                        return Properties.Resources.FONT_WEIGHT_SEMIBOLD;
                    case OnlyVFontWeight.Bold:
                        return Properties.Resources.FONT_WEIGHT_BOLD;
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
