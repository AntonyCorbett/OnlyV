namespace OnlyV.Extensions
{
    using Properties;
    using Serilog.Events;

    internal static class EnumExtensions
    {
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
