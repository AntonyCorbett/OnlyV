namespace OnlyV.Services.Options
{
    using System;
    using OnlyV.EventArgs;

    internal interface IOptionsService
    {
        event EventHandler<MonitorChangedEventArgs> MediaMonitorChangedEvent;

        AppOptions.Options Options { get; }

        void Save();
    }
}
