namespace OnlyV.Extensions
{
    using System.Windows;
    using Properties;
    using Serilog.Events;
    using Themes.Common.Specs;

    internal static class EnumExtensions
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

        public static string GetDescriptiveName(this LogEventLevel level)
        {
            switch (level)
            {
                case LogEventLevel.Debug:
                    return Resources.LOG_LEVEL_DEBUG;

                case LogEventLevel.Error:
                    return Resources.LOG_LEVEL_ERROR;

                case LogEventLevel.Fatal:
                    return Resources.LOG_LEVEL_FATAL;

                case LogEventLevel.Verbose:
                    return Resources.LOG_LEVEL_VERBOSE;

                case LogEventLevel.Warning:
                    return Resources.LOG_LEVEL_WARNING;

                default:
                // ReSharper disable once RedundantCaseLabel
                case LogEventLevel.Information:
                    return Resources.LOG_LEVEL_INFORMATION;
            }
        }
    }
}
