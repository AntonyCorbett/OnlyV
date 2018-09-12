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
        
        LogEventLevel LogEventLevel { get; set; }

        bool AlwaysOnTop { get; set; }

        string AppWindowPlacement { get; set; }

        string MediaMonitorId { get; set; }

        bool JwLibraryCompatibilityMode { get; set; }

        string EpubPath { get; set; }

        string ThemePath { get; set; }

        string SaveToFolder { get; set; }
        
        void Save();
    }
}
