namespace OnlyV.AppOptions
{
    using System.IO;
    using Serilog.Events;

    internal class Options
    {
        public Options()
        {
            AlwaysOnTop = true;
            LogEventLevel = LogEventLevel.Information;
            JwLibraryCompatibilityMode = true;
            UseBackgroundImage = true;
            AutoFit = true;
            ShowVerseBreaks = true;
            UseTildeMarker = true;
            ShowVerseBreaks = true;
            TextScalingPercentage = 100;

            Sanitize();
        }

        public string MediaMonitorId { get; set; }

        public bool AlwaysOnTop { get; set; }

        public bool AllowVerseEditing { get; set; }

        public string AppWindowPlacement { get; set; }

        public bool JwLibraryCompatibilityMode { get; set; }

        public LogEventLevel LogEventLevel { get; set; }

        public string SaveToFolder { get; set; }

        public string EpubPath { get; set; }

        public string ThemePath { get; set; }

        public bool UseBackgroundImage { get; set; }

        public bool AutoFit { get; set; }

        public bool ShowVerseBreaks { get; set; }

        public bool UseTildeMarker { get; set; }

        public bool TrimPunctuation { get; set; }

        public bool TrimQuotes { get; set; }
        
        public bool ShowVerseNos { get; set; }

        public int TextScalingPercentage { get; set; }

        /// <summary>
        /// Validates the data, correcting automatically as required
        /// </summary>
        public void Sanitize()
        {
            if (!File.Exists(EpubPath))
            {
                EpubPath = null;
            }

            if (!File.Exists(ThemePath))
            {
                ThemePath = null;
            }

            if (TextScalingPercentage < 50)
            {
                TextScalingPercentage = 50;
            }
            else if (TextScalingPercentage > 150)
            {
                TextScalingPercentage = 150;
            }
        }
    }
}
