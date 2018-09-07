namespace OnlyV.ViewModel
{
    using System.Collections.Generic;
    using System.Linq;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;
    using OnlyV.PubSubMessages;
    using OnlyV.Services.Monitors;
    using OnlyV.Services.Options;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SettingsViewModel : ViewModelBase
    {
        private readonly MonitorItem[] _monitors;

        private readonly IMonitorsService _monitorsService;
        private readonly IOptionsService _optionsService;

        public SettingsViewModel(
            IMonitorsService monitorsService,
            IOptionsService optionsService)
        {
            _monitorsService = monitorsService;
            _optionsService = optionsService;

            _monitors = GetSystemMonitors().ToArray();

            Messenger.Default.Register<ShutDownMessage>(this, OnShutDown);
        }

        public IEnumerable<MonitorItem> Monitors => _monitors;

        public string MonitorId
        {
            get => _optionsService.Options.MediaMonitorId;
            set
            {
                if (_optionsService.Options.MediaMonitorId != value)
                {
                    _optionsService.Options.MediaMonitorId = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IEnumerable<MonitorItem> GetSystemMonitors()
        {
            var result = new List<MonitorItem>
            {
                // empty (i.e. no timer monitor)
                new MonitorItem { MonitorName = Properties.Resources.MONITOR_NONE }
            };

            var monitors = _monitorsService.GetSystemMonitors();
            result.AddRange(monitors.Select(AutoMapper.Mapper.Map<MonitorItem>));

            return result;
        }

        private void OnShutDown(ShutDownMessage msg)
        {
            _optionsService.Save();
        }
    }
}
