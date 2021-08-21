using System;

namespace OnlyV.EventArguments
{
    internal class MonitorChangedEventArgs : EventArgs
    {
        public string OriginalMonitorId { get; set; }

        public string NewMonitorId { get; set; }
    }
}
