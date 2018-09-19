namespace OnlyV.Themes.Common.Extensions
{
    using System.Windows;
    using Specs;

    public static class EnumExtensions
    {
        public static TextAlignment AsWindowsTextAlignment(this OnlyVHorizontalTextAlignment alignment)
        {
            switch (alignment)
            {
                case OnlyVHorizontalTextAlignment.Left:
                    return TextAlignment.Left;

                case OnlyVHorizontalTextAlignment.Right:
                    return TextAlignment.Right;

                default:
                case OnlyVHorizontalTextAlignment.Centre:
                    return TextAlignment.Center;
            }
        }

        public static FontWeight AsWindowsFontWeight(this OnlyVFontWeight weight)
        {
            switch (weight)
            {
                case OnlyVFontWeight.Light:
                    return FontWeights.Light;

                case OnlyVFontWeight.SemiBold:
                    return FontWeights.SemiBold;

                case OnlyVFontWeight.Bold:
                    return FontWeights.Bold;

                default:
                case OnlyVFontWeight.Normal:
                    return FontWeights.Normal;
            }
        }

        public static FontStyle AsWindowsFontStyle(this OnlyVFontStyle style)
        {
            switch (style)
            {
                case OnlyVFontStyle.Italic:
                    return FontStyles.Italic;

                case OnlyVFontStyle.Oblique:
                    return FontStyles.Oblique;

                default:
                case OnlyVFontStyle.Normal:
                    return FontStyles.Normal;
            }
        }
    }
}
