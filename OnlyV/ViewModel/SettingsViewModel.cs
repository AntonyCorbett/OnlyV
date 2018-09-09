namespace OnlyV.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;
    using GalaSoft.MvvmLight.Threading;
    using Helpers;
    using PubSubMessages;
    using Serilog;
    using Services.Bible;
    using Services.DragDrop;
    using Services.Monitors;
    using Services.Options;
    using Services.Snackbar;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SettingsViewModel : ViewModelBase
    {
        private readonly MonitorItem[] _monitors;
        
        private readonly IMonitorsService _monitorsService;
        private readonly IOptionsService _optionsService;
        private readonly IDragDropService _dragDropService;
        private readonly IBibleVersesService _bibleVersesService;
        private readonly ISnackbarService _snackbarService;

        private EpubFileItem[] _bibleEpubFiles;

        public SettingsViewModel(
            IMonitorsService monitorsService,
            IOptionsService optionsService,
            IDragDropService dragDropService,
            IBibleVersesService bibleVersesService,
            ISnackbarService snackbarService)
        {
            _monitorsService = monitorsService;
            _optionsService = optionsService;
            _dragDropService = dragDropService;
            _bibleVersesService = bibleVersesService;
            _snackbarService = snackbarService;

            _monitors = GetSystemMonitors().ToArray();
            _bibleEpubFiles = GetBibleEpubFiles().ToArray();

            Messenger.Default.Register<DragOverMessage>(this, OnDragOver);
            Messenger.Default.Register<DragDropMessage>(this, OnDragDrop);
            Messenger.Default.Register<ShutDownMessage>(this, OnShutDown);
        }

        public event EventHandler EpubChangedEvent;

        public IEnumerable<EpubFileItem> BibleEpubFiles => _bibleEpubFiles;

        public string CurrentEpubFilePath
        {
            get => _optionsService.Options.EpubPath;
            set
            {
                if (_optionsService.Options.EpubPath != value)
                {
                    _optionsService.Options.EpubPath = value;
                    RaisePropertyChanged();

                    EpubChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
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

        private void OnDragDrop(DragDropMessage message)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            var origEpubPath = _bibleVersesService.EpubPath;

            Task.Run(() =>
            {
                var files = _dragDropService.GetDroppedFiles(message.DragEventArgs);
                
                var validFileCount = 0;
                
                // close reader so that we can overwrite the current epub (if necessary)
                _bibleVersesService.CloseReader();
                
                Parallel.ForEach(files, file =>
                {
                    try
                    {
                        if (_bibleVersesService.IsValidBibleEpub(file))
                        {
                            Interlocked.Increment(ref validFileCount);
                            File.Copy(file, GetDestinationFileName(file), overwrite: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error($@"Could not copy epub file {file}", ex);
                    }
                });

                _snackbarService.EnqueueWithOk(GetDoneMessage(files.Count, validFileCount));
                message.DragEventArgs.Handled = true;
            }).ContinueWith(t =>
            {
                _bibleVersesService.EpubPath = origEpubPath;

                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    Mouse.OverrideCursor = null;
                    UpdateBibleEpubsList();
                });
            });
        }

        private void UpdateBibleEpubsList()
        {
            var currentSelection = CurrentEpubFilePath;

            _bibleEpubFiles = GetBibleEpubFiles().ToArray();
            RaisePropertyChanged(nameof(BibleEpubFiles));

            CurrentEpubFilePath = currentSelection;
        }

        private string GetDoneMessage(int fileCount, int validFileCount)
        {
            if (fileCount == 0)
            {
                return Properties.Resources.COULD_NOT_READ_FILE;
            }

            if (validFileCount == 0)
            {
                return fileCount == 1 
                    ? Properties.Resources.COULD_NOT_READ_FILE 
                    : Properties.Resources.COULD_NOT_READ_ANY;
            }

            if (validFileCount < fileCount)
            {
                var badFileCount = fileCount - validFileCount;

                return badFileCount == 1 
                    ? Properties.Resources.COULD_NOT_READ_1 
                    : string.Format(Properties.Resources.COULD_NOT_READ_X, badFileCount);
            }

            return validFileCount == 1 
                ? Properties.Resources.ADDED_FILE 
                : string.Format(Properties.Resources.ADDED_X_FILES, validFileCount);
        }

        private string GetDestinationFileName(string file)
        {
            var filename = Path.GetFileName(file);
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException(nameof(file));
            }

            return Path.Combine(FileUtils.GetEpubFolder(), filename);
        }

        private void OnDragOver(DragOverMessage message)
        {
            message.DragEventArgs.Effects = _dragDropService.CanAcceptDrop(message.DragEventArgs)
                ? DragDropEffects.Copy
                : DragDropEffects.None;

            message.DragEventArgs.Handled = true;
        }
    }
}
