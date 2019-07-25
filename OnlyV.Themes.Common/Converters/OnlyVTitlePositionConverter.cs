namespace OnlyV.Themes.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using OnlyV.Themes.Common.Specs;

    public class OnlyVTitlePositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OnlyVTitlePosition position)
            {
                switch (position)
                {
                    case OnlyVTitlePosition.Top:
                        return Properties.Resources.TITLE_POS_TOP;

                    case OnlyVTitlePosition.Bottom:
                        return Properties.Resources.TITLE_POS_BOTTOM;
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
