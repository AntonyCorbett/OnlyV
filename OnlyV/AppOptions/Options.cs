namespace OnlyV.AppOptions
{
    using System;
    using EventArgs;
    using Serilog.Events;

    internal class Options
    {
        private string _mediaMonitorId;
        private bool _permanentBackdrop;
        private bool _alwaysOnTop;
        private LogEventLevel _logEventLevel;

        public Options()
        {
            AlwaysOnTop = true;
            LogEventLevel = LogEventLevel.Information;
            PermanentBackdrop = true;
            JwLibraryCompatibilityMode = true;

            Sanitize();
        }

        public event EventHandler PermanentBackdropChangedEvent;

        public event EventHandler AlwaysOnTopChangedEvent;

        public event EventHandler LogEventLevelChangedEvent;

        public event EventHandler<MonitorChangedEventArgs> MediaMonitorChangedEvent;

        public string MediaMonitorId
        {
            get => _mediaMonitorId;
            set
            {
                if (_mediaMonitorId != value)
                {
                    var originalMonitorId = _mediaMonitorId;
                    _mediaMonitorId = value;
                    OnMediaMonitorChangedEvent(originalMonitorId, value);
                }
            }
        }

        public bool PermanentBackdrop
        {
            get => _permanentBackdrop;
            set
            {
                if (_permanentBackdrop != value)
                {
                    _permanentBackdrop = value;
                    PermanentBackdropChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool AlwaysOnTop
        {
            get => _alwaysOnTop;
            set
            {
                if (_alwaysOnTop != value)
                {
                    _alwaysOnTop = value;
                    AlwaysOnTopChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public string AppWindowPlacement { get; set; }

        public bool JwLibraryCompatibilityMode { get; set; }

        public LogEventLevel LogEventLevel
        {
            get => _logEventLevel;
            set
            {
                if (_logEventLevel != value)
                {
                    _logEventLevel = value;
                    LogEventLevelChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Validates the data, correcting automatically as required
        /// </summary>
        public void Sanitize()
        {
            if (JwLibraryCompatibilityMode)
            {
                PermanentBackdrop = false;
            }
        }

        private void OnMediaMonitorChangedEvent(string originalMonitorId, string newMonitorId)
        {
            MediaMonitorChangedEvent?.Invoke(
                this,
                new MonitorChangedEventArgs
                {
                    OriginalMonitorId = originalMonitorId,
                    NewMonitorId = newMonitorId
                });
        }
    }
}
