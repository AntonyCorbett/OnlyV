using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using GalaSoft.MvvmLight.Threading;
using Newtonsoft.Json;
using OnlyV.EventArguments;
using OnlyV.Helpers;
using OnlyV.Services.CommandLine;
using OnlyV.Services.LoggingLevel;
using OnlyV.Services.Monitors;
using OnlyV.Themes.Common.Services;
using Serilog;
using Serilog.Events;

namespace OnlyV.Services.Options
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class OptionsService : IOptionsService
    {
        private readonly ICommandLineService _commandLineService;
        private readonly ILogLevelSwitchService _logLevelSwitchService;
        private readonly IMonitorsService _monitorsService;
        private readonly int _optionsVersion = 1;
        private readonly SingleExecAction _textScalingAction = new SingleExecAction(TimeSpan.FromMilliseconds(500));

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

        public event EventHandler ThemePathChangedEvent;

        public event EventHandler StyleChangedEvent;

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

        public bool AllowVerseEditing
        {
            get => _options.AllowVerseEditing;
            set
            {
                if (_options.AllowVerseEditing != value)
                {
                    _options.AllowVerseEditing = value;
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

        public int TextScalingPercentage
        {
            get => _options.TextScalingPercentage;
            set
            {
                if (_options.TextScalingPercentage != value)
                {
                    _options.TextScalingPercentage = value;
                    _textScalingAction.Execute(() =>
                    {
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            StyleChangedEvent?.Invoke(this, EventArgs.Empty);
                        });
                    });
                }
            }
        }

        public bool JwLibraryCompatibilityMode
        {
            get => _options.JwLibraryCompatibilityMode;
            set
            {
                if (_options.JwLibraryCompatibilityMode != value)
                {
                    _options.JwLibraryCompatibilityMode = value;
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

        public string ThemePath
        {
            get => _options.ThemePath;
            set
            {
                if (_options.ThemePath == null || _options.ThemePath != value)
                {
                    _options.ThemePath = value;
                    ThemePathChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool UseBackgroundImage
        {
            get => _options.UseBackgroundImage;
            set
            {
                if (_options.UseBackgroundImage != value)
                {
                    _options.UseBackgroundImage = value;
                    StyleChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool AutoFit
        {
            get => _options.AutoFit;
            set
            {
                if (_options.AutoFit != value)
                {
                    _options.AutoFit = value;
                    StyleChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool ShowVerseBreaks
        {
            get => _options.ShowVerseBreaks;
            set
            {
                if (_options.ShowVerseBreaks != value)
                {
                    _options.ShowVerseBreaks = value;
                    StyleChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool UseContinuationEllipses
        {
            get => _options.UseContinuationEllipses;
            set
            {
                if (_options.UseContinuationEllipses != value)
                {
                    _options.UseContinuationEllipses = value;
                    StyleChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool UseAbbreviatedBookNames
        {
            get => _options.UseAbbreviatedBookNames;
            set
            {
                if (_options.UseAbbreviatedBookNames != value)
                {
                    _options.UseAbbreviatedBookNames = value;
                    StyleChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool UseTildeMarker
        {
            get => _options.UseTildeMarker;
            set
            {
                if (_options.UseTildeMarker != value)
                {
                    _options.UseTildeMarker = value;
                    StyleChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool TrimPunctuation
        {
            get => _options.TrimPunctuation;
            set
            {
                if (_options.TrimPunctuation != value)
                {
                    _options.TrimPunctuation = value;
                    StyleChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool SpaceBetweenTitleVerseNumbers
        {
            get => _options.SpaceBetweenTitleVerseNumbers;
            set
            {
                if (_options.SpaceBetweenTitleVerseNumbers != value)
                {
                    _options.SpaceBetweenTitleVerseNumbers = value;
                    StyleChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool TrimQuotes
        {
            get => _options.TrimQuotes;
            set
            {
                if (_options.TrimQuotes != value)
                {
                    _options.TrimQuotes = value;
                    StyleChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool ShowVerseNos
        {
            get => _options.ShowVerseNos;
            set
            {
                if (_options.ShowVerseNos != value)
                {
                    _options.ShowVerseNos = value;
                    StyleChangedEvent?.Invoke(this, EventArgs.Empty);
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

        public string Culture
        {
            get => _options.Culture;
            set
            {
                if (_options.Culture == null || _options.Culture != value)
                {
                    _options.Culture = value;
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

        private static string GetOptionsSignature(AppOptions.Options options)
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

                    SetCulture();
                }
            }
        }

        private void SetCulture()
        {
            var culture = _options.Culture;

            if (string.IsNullOrEmpty(culture))
            {
                culture = CultureInfo.CurrentCulture.Name;
            }

            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(
                        XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not set culture");
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
