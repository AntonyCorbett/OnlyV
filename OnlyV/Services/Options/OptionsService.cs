namespace OnlyV.Services.Options
{
    using System;
    using System.IO;
    using System.Linq;
    using CommandLine;
    using EventArgs;
    using Helpers;
    using LoggingLevel;
    using Monitors;
    using Newtonsoft.Json;
    using Serilog;
    using Serilog.Events;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class OptionsService : IOptionsService
    {
        private readonly ICommandLineService _commandLineService;
        private readonly ILogLevelSwitchService _logLevelSwitchService;
        private readonly IMonitorsService _monitorsService;
        private readonly int _optionsVersion = 1;
        
        private AppOptions.Options _options;
        private string _optionsFilePath;
        private string _originalOptionsSignature;

        public OptionsService(
            ICommandLineService commandLineService,
            ILogLevelSwitchService logLevelSwitchService,
            IMonitorsService monitorsService)
        {
            _commandLineService = commandLineService;
            _logLevelSwitchService = logLevelSwitchService;
            _monitorsService = monitorsService;

            Init();
        }

        public event EventHandler<MonitorChangedEventArgs> MediaMonitorChangedEvent;

        public event EventHandler AlwaysOnTopChangedEvent;
        
        public event EventHandler EpubPathChangedEvent;

        public string MediaMonitorId
        {
            get => _options.MediaMonitorId;
            set
            {
                if (_options.MediaMonitorId != value)
                {
                    var originalMonitorId = _options.MediaMonitorId;
                    _options.MediaMonitorId = value;

                    MediaMonitorChangedEvent?.Invoke(this, new MonitorChangedEventArgs
                    {
                        OriginalMonitorId = originalMonitorId,
                        NewMonitorId = value
                    });
                }
            }
        }

        public bool AlwaysOnTop
        {
            get => _options.AlwaysOnTop;
            set
            {
                if (_options.AlwaysOnTop != value)
                {
                    _options.AlwaysOnTop = value;
                    AlwaysOnTopChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public string AppWindowPlacement
        {
            get => _options.AppWindowPlacement;
            set
            {
                if (_options.AppWindowPlacement == null || _options.AppWindowPlacement != value)
                {
                    _options.AppWindowPlacement = value;
                }
            }
        }

        public LogEventLevel LogEventLevel
        {
            get => _options.LogEventLevel;
            set
            {
                if (_options.LogEventLevel != value)
                {
                    _options.LogEventLevel = value;
                    _logLevelSwitchService.SetMinimumLevel(value);
                }
            }
        }

        public bool JwLibraryCompatibilityMode
        {
            get => _options.JwLibraryCompatibilityMode;
            set
            {
                if (JwLibraryCompatibilityMode != value)
                {
                    JwLibraryCompatibilityMode = value;
                }
            }
        }

        public string EpubPath
        {
            get => _options.EpubPath;
            set
            {
                if (_options.EpubPath == null || _options.EpubPath != value)
                {
                    _options.EpubPath = value;
                    EpubPathChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public string SaveToFolder
        {
            get => _options.SaveToFolder;
            set
            {
                if (_options.SaveToFolder == null || _options.SaveToFolder != value)
                {
                    _options.SaveToFolder = value;
                }
            }
        }

        /// <summary>
        /// Saves the settings (if they have changed since they were last read)
        /// </summary>
        public void Save()
        {
            try
            {
                var newSignature = GetOptionsSignature(_options);
                if (_originalOptionsSignature != newSignature)
                {
                    // changed...
                    WriteOptions();
                    Log.Logger.Information("Settings changed and saved");
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not save settings");
            }
        }

        private string GetOptionsSignature(AppOptions.Options options)
        {
            // config data is small so simple solution is best...
            return JsonConvert.SerializeObject(options);
        }

        private void Init()
        {
            if (_options == null)
            {
                try
                {
                    var commandLineIdentifier = _commandLineService.OptionsIdentifier;
                    _optionsFilePath = FileUtils.GetUserOptionsFilePath(commandLineIdentifier, _optionsVersion);
                    var path = Path.GetDirectoryName(_optionsFilePath);
                    if (path != null)
                    {
                        FileUtils.CreateDirectory(path);
                        ReadOptions();
                    }

                    if (_options == null)
                    {
                        _options = new AppOptions.Options();
                    }
                    
                    // store the original settings so that we can determine if they have changed
                    // when we come to save them
                    _originalOptionsSignature = GetOptionsSignature(_options);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Could not read options file");
                    _options = new AppOptions.Options();
                }
            }
        }

        private void ReadOptions()
        {
            if (!File.Exists(_optionsFilePath))
            {
                WriteDefaultOptions();
            }
            else
            {
                using (var file = File.OpenText(_optionsFilePath))
                {
                    var serializer = new JsonSerializer();
                    _options = (AppOptions.Options)serializer.Deserialize(file, typeof(AppOptions.Options));

                    _options.Sanitize();
                }
            }
        }

        private void WriteDefaultOptions()
        {
            _options = new AppOptions.Options();

            var monitor = _monitorsService.GetSystemMonitors()?.FirstOrDefault();
            if (monitor != null)
            {
                _options.MediaMonitorId = monitor.MonitorId;
            }

            WriteOptions();
        }

        private void WriteOptions()
        {
            if (_options != null)
            {
                using (var file = File.CreateText(_optionsFilePath))
                {
                    var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                    serializer.Serialize(file, _options);
                    _originalOptionsSignature = GetOptionsSignature(_options);
                }
            }
        }
    }
}
