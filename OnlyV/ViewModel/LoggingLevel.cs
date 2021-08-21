using Serilog.Events;

namespace OnlyV.ViewModel
{
    internal class LoggingLevel
    {
        public string Name { get; set; }

        public LogEventLevel Level { get; set; }
    }
}
