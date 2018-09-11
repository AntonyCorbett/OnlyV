namespace OnlyV.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Extensions;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;
    using Helpers;
    using PubSubMessages;
    using Serilog.Events;
    using Services.Bible;
    using Services.DragDrop;
    using Services.Monitors;
    using Services.Options;
    using Services.Snackbar;
    using Services.UI;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SettingsViewModel : ViewModelBase
    {
        private readonly MonitorItem[] _monitors;
        private readonly LoggingLevel[] _loggingLevels;

        private readonly IMonitorsService _monitorsService;
        private readonly IOptionsService _optionsService;
        private readonly IDragDropService _dragDropService;
        private readonly IUserInterfaceService _userInterfaceService;

        private EpubFileItem[] _bibleEpubFiles;

        public SettingsViewModel(
            IMonitorsService monitorsService,
            IOptionsService optionsService,
            IDragDropService dragDropService,
            IUserInterfaceService userInterfaceService)
        {
            _monitorsService = monitorsService;
            _optionsService = optionsService;
            _dragDropService = dragDropService;
            _userInterfaceService = userInterfaceService;

            dragDropService.EpubFileListChanged += HandleEpubFileListChanged;

            _monitors = GetSystemMonitors().ToArray();
            _loggingLevels = GetLoggingLevels().ToArray();
            _bibleEpubFiles = GetBibleEpubFiles().ToArray();

            Messenger.Default.Register<ShutDownMessage>(this, OnShutDown);
        }

        public event EventHandler EpubChangedEvent;

        public IEnumerable<EpubFileItem> BibleEpubFiles => _bibleEpubFiles;

        public string CurrentEpubFilePath
        {
            get => _optionsService.EpubPath;
            set
            {
                if (_optionsService.EpubPath != value)
                {
                    _optionsService.EpubPath = value;
                    RaisePropertyChanged();

                    using (_userInterfaceService.GetBusy())
                    {
                        EpubChangedEvent?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        public string AppVersionStr => string.Format(Properties.Resources.APP_VER, VersionDetection.GetCurrentVersion());
        
        public bool AlwaysOnTop
        {
            get => _optionsService.AlwaysOnTop;
            set
            {
                if (_optionsService.AlwaysOnTop != value)
                {
                    _optionsService.AlwaysOnTop = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IEnumerable<MonitorItem> Monitors => _monitors;

        public string MonitorId
        {
            get => _optionsService.MediaMonitorId;
            set
            {
                if (_optionsService.MediaMonitorId != value)
                {
                    _optionsService.MediaMonitorId = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool JwLibraryCompatibilityMode
        {
            get => _optionsService.JwLibraryCompatibilityMode;
            set
            {
                if (_optionsService.JwLibraryCompatibilityMode != value)
                {
                    _optionsService.JwLibraryCompatibilityMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IEnumerable<LoggingLevel> LoggingLevels => _loggingLevels;

        public LogEventLevel LogEventLevel
        {
            get => _optionsService.LogEventLevel;
            set
            {
                if (_optionsService.LogEventLevel != value)
                {
                    _optionsService.LogEventLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IEnumerable<LoggingLevel> GetLoggingLevels()
        {
            var result = new List<LoggingLevel>();

            foreach (LogEventLevel v in Enum.GetValues(typeof(LogEventLevel)))
            {
                result.Add(new LoggingLevel
                {
                    Level = v,
                    Name = v.GetDescriptiveName()
                });
            }

            return result;
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

        private IReadOnlyCollection<EpubFileItem> GetBibleEpubFiles()
        {
            var result = new List<EpubFileItem>();

            var files = Directory.GetFiles(FileUtils.GetEpubFolder(), "*.epub").ToList();

            foreach (var file in files)
            {
                result.Add(new EpubFileItem
                {
                    Path = file,
                    Name = Path.GetFileNameWithoutExtension(file)
                });
            }

            result.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
            return result;
        }

        private void OnShutDown(ShutDownMessage msg)
        {
            _optionsService.Save();
        }

        private void HandleEpubFileListChanged(object sender, EventArgs e)
        {
            var currentSelection = CurrentEpubFilePath;

            _bibleEpubFiles = GetBibleEpubFiles().ToArray();
            RaisePropertyChanged(nameof(BibleEpubFiles));

            if (currentSelection == null)
            {
                if (_bibleEpubFiles.Any())
                {
                    CurrentEpubFilePath = _bibleEpubFiles.First().Path;
                }
            }
            else
            {
                CurrentEpubFilePath = currentSelection;
            }
        }
    }
}
