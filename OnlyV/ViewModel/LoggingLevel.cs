namespace OnlyV.ViewModel
{
    using Serilog.Events;

    internal class LoggingLevel
    {
        public string Name { get; set; }

        public LogEventLevel Level { get; set; }
    }
}
