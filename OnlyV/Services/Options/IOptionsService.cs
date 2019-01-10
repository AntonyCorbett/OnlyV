namespace OnlyV.Services.Options
{
    using System;
    using EventArgs;
    using Serilog.Events;

    internal interface IOptionsService
    {
        event EventHandler<MonitorChangedEventArgs> MediaMonitorChangedEvent;
        
        event EventHandler AlwaysOnTopChangedEvent;
        
        event EventHandler EpubPathChangedEvent;

        event EventHandler ThemePathChangedEvent;

        event EventHandler StyleChangedEvent;

        LogEventLevel LogEventLevel { get; set; }

        bool AlwaysOnTop { get; set; }

        bool AllowVerseEditing { get; set; }

        string AppWindowPlacement { get; set; }

        string MediaMonitorId { get; set; }

        bool JwLibraryCompatibilityMode { get; set; }

        string EpubPath { get; set; }

        string ThemePath { get; set; }

        bool UseBackgroundImage { get; set; }

        bool AutoFit { get; set; }

        bool ShowVerseBreaks { get; set; }

        bool UseTildeMarker { get; set; }

        bool TrimPunctuation { get; set; }

        bool TrimQuotes { get; set; }
        
        bool ShowVerseNos { get; set; }

        bool SpaceBetweenTitleVerseNumbers { get; set; }

        string SaveToFolder { get; set; }

        int TextScalingPercentage { get; set; }

        void Save();
    }
}
