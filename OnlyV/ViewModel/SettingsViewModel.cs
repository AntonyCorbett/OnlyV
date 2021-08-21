using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using OnlyV.Extensions;
using OnlyV.Helpers;
using OnlyV.PubSubMessages;
using OnlyV.Services.DragDrop;
using OnlyV.Services.Monitors;
using OnlyV.Services.Options;
using OnlyV.Themes.Common.FileHandling;
using OnlyV.Themes.Common.Models;
using OnlyV.Themes.Common.Services.UI;
using Serilog.Events;

namespace OnlyV.ViewModel
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SettingsViewModel : ViewModelBase
    {
        private readonly MonitorItem[] _monitors;
        private readonly LoggingLevel[] _loggingLevels;
        private readonly LanguageItem[] _languages;

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

            _monitors = GetSystemMonitors();
            _languages = GetSupportedLanguages();
            _loggingLevels = GetLoggingLevels();
            _bibleEpubFiles = GetBibleEpubFiles();
            _themeFiles = GetThemeFiles();

            SelectDestinationFolderCommand = new RelayCommand(SelectDestinationFolder);
            ResetTextScalingCommand = new RelayCommand(ResetTextScaling, CanResetScaling);

            Messenger.Default.Register<ShutDownMessage>(this, OnShutDown);
        }

        public event EventHandler EpubChangedEvent;

        public event EventHandler ThemeChangedEvent;

        public RelayCommand SelectDestinationFolderCommand { get; set; }

        public RelayCommand ResetTextScalingCommand { get; set; }

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

        public IEnumerable<LanguageItem> Languages => _languages;

        public string LanguageId
        {
            get => _optionsService.Culture;
            set
            {
                if (_optionsService.Culture != value)
                {
                    _optionsService.Culture = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IEnumerable<EpubFileItem> BibleEpubFiles => _bibleEpubFiles;

        public string DestinationFolder
        {
            get => _optionsService.SaveToFolder;
            set
            {
                if (_optionsService.SaveToFolder != value)
                {
                    _optionsService.SaveToFolder = value;
                    RaisePropertyChanged(nameof(DestinationFolder));
                }
            }
        }

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

        public bool AllowVerseEditing
        {
            get => _optionsService.AllowVerseEditing;
            set
            {
                if (_optionsService.AllowVerseEditing != value)
                {
                    _optionsService.AllowVerseEditing = value;
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

        public bool SpaceBetweenTitleVerseNumbers
        {
            get => _optionsService.SpaceBetweenTitleVerseNumbers;
            set
            {
                if (_optionsService.SpaceBetweenTitleVerseNumbers != value)
                {
                    _optionsService.SpaceBetweenTitleVerseNumbers = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool ShowEllipsesForContinuation
        {
            get => _optionsService.UseContinuationEllipses;
            set
            {
                if (_optionsService.UseContinuationEllipses != value)
                {
                    _optionsService.UseContinuationEllipses = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool UseAbbreviatedBookNames
        {
            get => _optionsService.UseAbbreviatedBookNames;
            set
            {
                if (_optionsService.UseAbbreviatedBookNames != value)
                {
                    _optionsService.UseAbbreviatedBookNames = value;
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

        private static LoggingLevel[] GetLoggingLevels()
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

            return result.ToArray();
        }

        private MonitorItem[] GetSystemMonitors()
        {
            var result = new List<MonitorItem>
            {
                // empty (i.e. no timer monitor)
                new MonitorItem
                {
                    MonitorName = Properties.Resources.MONITOR_NONE,
                    FriendlyName = Properties.Resources.MONITOR_NONE
                }
            };

            var monitors = _monitorsService.GetSystemMonitors();
            result.AddRange(monitors.Select(ToMonitorItem));

            return result.ToArray();
        }

        private MonitorItem ToMonitorItem(SystemMonitor monitor)
        {
            return new MonitorItem
            {
                FriendlyName = monitor.FriendlyName,
                Monitor = monitor.Monitor,
                MonitorId = monitor.MonitorId,
                MonitorName = monitor.MonitorName
            };
        }

        private static EpubFileItem[] GetBibleEpubFiles()
        {
            var result = new List<EpubFileItem>();

            foreach (var file in Directory.GetFiles(FileUtils.GetEpubFolder(), "*.epub"))
            {
                result.Add(new EpubFileItem
                {
                    Path = file,
                    Name = Path.GetFileNameWithoutExtension(file)
                });
            }

            result.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
            return result.ToArray();
        }

        private static ThemeFileItem[] GetThemeFiles()
        {
            var result = new List<ThemeFileItem>();
            
            var allThemes = new List<string>();

            var stdThemeFolder = FileUtils.GetStandardThemeFolder();
            if (stdThemeFolder != null)
            {
                allThemes.AddRange(Directory.GetFiles(stdThemeFolder, $"*{ThemeFile.ThemeFileExtension}"));
            }

            allThemes.AddRange(Directory.GetFiles(FileUtils.GetPrivateThemeFolder(), $"*{ThemeFile.ThemeFileExtension}"));

            foreach (var file in allThemes)
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
            return result.ToArray();
        }

        private void OnShutDown(ShutDownMessage msg)
        {
            _optionsService.Save();
        }

        private void HandleEpubFileListChanged(object sender, System.EventArgs e)
        {
            var currentSelection = CurrentEpubFilePath;

            _bibleEpubFiles = GetBibleEpubFiles();
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

        private void SelectDestinationFolder()
        {
            using (var d = new CommonOpenFileDialog(Properties.Resources.SELECT_FOLDER) { IsFolderPicker = true })
            {
                var result = d.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    DestinationFolder = d.FileName;
                }
            }
        }

        private static LanguageItem[] GetSupportedLanguages()
        {
            var result = new List<LanguageItem>();

            var subFolders = Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory);

            foreach (var folder in subFolders)
            {
                if (!string.IsNullOrEmpty(folder))
                {
                    try
                    {
                        var c = new CultureInfo(Path.GetFileNameWithoutExtension(folder));
                        result.Add(new LanguageItem
                        {
                            LanguageId = c.Name,
                            LanguageName = c.EnglishName
                        });
                    }
                    catch (CultureNotFoundException)
                    {
                        // expected
                    }
                }
            }

            // the native language
            {
                var c = new CultureInfo(Path.GetFileNameWithoutExtension("en-GB"));
                result.Add(new LanguageItem
                {
                    LanguageId = c.Name,
                    LanguageName = c.EnglishName
                });
            }

            result.Sort((x, y) => string.Compare(x.LanguageName, y.LanguageName, StringComparison.Ordinal));

            return result.ToArray();
        }

        private void ResetTextScaling()
        {
            TextScalingPercentage = 100;
        }

        private bool CanResetScaling()
        {
            return TextScalingPercentage != 100.0;
        }
    }
}
