namespace OnlyV.EventArgs
{
    internal class MonitorChangedEventArgs : System.EventArgs
    {
        public string OriginalMonitorId { get; set; }

        public string NewMonitorId { get; set; }
    }
}
