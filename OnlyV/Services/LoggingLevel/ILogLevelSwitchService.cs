using Serilog.Events;

namespace OnlyV.Services.LoggingLevel
{
    internal interface ILogLevelSwitchService
    {
        void SetMinimumLevel(LogEventLevel level);
    }
}
