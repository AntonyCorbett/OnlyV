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
    using Services.DragDrop;
    using Services.Monitors;
    using Services.Options;
    using Services.UI;
    using Themes.Common.FileHandling;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SettingsViewModel : ViewModelBase
    {
        private readonly MonitorItem[] _monitors;
        private readonly LoggingLevel[] _loggingLevels;

        private readonly IMonitorsService _monitorsService;
        private readonly IOptionsService _optionsService;
        private readonly IUserInterfaceService _userInterfaceService;
        private readonly ThemeFileItem[] _themeFiles;

        private EpubFileItem[] _bibleEpubFiles;
        
        public SettingsViewModel(
            IMonitorsService monitorsService,
            IOptionsService optionsService,
            IDragDropService dragDropService,
            IUserInterfaceService userInterfaceService)
        {
            _monitorsService = monitorsService;
            _optionsService = optionsService;
            _userInterfaceService = userInterfaceService;

            dragDropService.EpubFileListChanged += HandleEpubFileListChanged;

            _monitors = GetSystemMonitors().ToArray();
            _loggingLevels = GetLoggingLevels().ToArray();
            _bibleEpubFiles = GetBibleEpubFiles().ToArray();
            _themeFiles = GetThemeFiles().ToArray();

            Messenger.Default.Register<ShutDownMessage>(this, OnShutDown);
        }

        public event EventHandler EpubChangedEvent;

        public event EventHandler ThemeChangedEvent;

        public IEnumerable<ThemeFileItem> ThemeFiles => _themeFiles;

        public string CurrentThemePath
        {
            get => _optionsService.ThemePath;
            set
            {
                if (_optionsService.ThemePath != value)
                {
                    _optionsService.ThemePath = value;
                    RaisePropertyChanged();

                    using (_userInterfaceService.GetBusy())
                    {
                        ThemeChangedEvent?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

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

        public double TextScalingPercentage
        {
            get => _optionsService.TextScalingPercentage;
            set
            {
                if (_optionsService.TextScalingPercentage != (int)value)
                {
                    _optionsService.TextScalingPercentage = (int)value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool UseBackgroundImage
        {
            get => _optionsService.UseBackgroundImage;
            set
            {
                if (_optionsService.UseBackgroundImage != value)
                {
                    _optionsService.UseBackgroundImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool AutoFit
        {
            get => _optionsService.AutoFit;
            set
            {
                if (_optionsService.AutoFit != value)
                {
                    _optionsService.AutoFit = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool ShowVerseBreaks
        {
            get => _optionsService.ShowVerseBreaks;
            set
            {
                if (_optionsService.ShowVerseBreaks != value)
                {
                    _optionsService.ShowVerseBreaks = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool UseTildeMarker
        {
            get => _optionsService.UseTildeMarker;
            set
            {
                if (_optionsService.UseTildeMarker != value)
                {
                    _optionsService.UseTildeMarker = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool TrimPunctuation
        {
            get => _optionsService.TrimPunctuation;
            set
            {
                if (_optionsService.TrimPunctuation != value)
                {
                    _optionsService.TrimPunctuation = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool TrimQuotes
        {
            get => _optionsService.TrimQuotes;
            set
            {
                if (_optionsService.TrimQuotes != value)
                {
                    _optionsService.TrimQuotes = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool ShowVerseNos
        {
            get => _optionsService.ShowVerseNos;
            set
            {
                if (_optionsService.ShowVerseNos != value)
                {
                    _optionsService.ShowVerseNos = value;
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

        private IReadOnlyCollection<ThemeFileItem> GetThemeFiles()
        {
            var result = new List<ThemeFileItem>();

            var files = Directory.GetFiles(FileUtils.GetThemeFolder(), $"*{ThemeFile.ThemeFileExtension}").ToList();

            foreach (var file in files)
            {
                result.Add(new ThemeFileItem
                {
                    Path = file,
                    Name = Path.GetFileNameWithoutExtension(file)
                });
            }

            result.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

            // insert the default theme.
            result.Insert(0, new ThemeFileItem { Name = Properties.Resources.DEFAULT_THEME });
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
