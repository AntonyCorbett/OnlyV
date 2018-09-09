namespace OnlyV.Services.LoggingLevel
{
    using Serilog.Events;

    internal interface ILogLevelSwitchService
    {
        void SetMinimumLevel(LogEventLevel level);
    }
}
