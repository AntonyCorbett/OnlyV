namespace OnlyV.AppOptions
{
    using System;
    using EventArgs;
    using Serilog.Events;

    internal class Options
    {
        public Options()
        {
            AlwaysOnTop = true;
            LogEventLevel = LogEventLevel.Information;
            JwLibraryCompatibilityMode = true;

            Sanitize();
        }

        public string MediaMonitorId { get; set; }

        public bool AlwaysOnTop { get; set; }

        public string AppWindowPlacement { get; set; }

        public bool JwLibraryCompatibilityMode { get; set; }

        public LogEventLevel LogEventLevel { get; set; }

        public string SaveToFolder { get; set; }

        public string EpubPath { get; set; }

        /// <summary>
        /// Validates the data, correcting automatically as required
        /// </summary>
        public void Sanitize()
        {
        }
    }
}
