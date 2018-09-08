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
    using Services.Bible;
    using Services.DragDrop;
    using Services.Monitors;
    using Services.Options;
    using Services.Snackbar;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SettingsViewModel : ViewModelBase
    {
        private readonly MonitorItem[] _monitors;
        private readonly EpubFileItem[] _bibleEpubFiles;

        private readonly IMonitorsService _monitorsService;
        private readonly IOptionsService _optionsService;
        private readonly IDragDropService _dragDropService;
        private readonly IBibleVersesService _bibleVersesService;
        private readonly ISnackbarService _snackbarService;

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

            Task.Run(() =>
            {
                var files = _dragDropService.GetDroppedFiles(message.DragEventArgs);
                
                var validFileCount = 0;

                Parallel.ForEach(files, file =>
                {
                    if (_bibleVersesService.IsValidBibleEpub(file))
                    {
                        Interlocked.Increment(ref validFileCount);
                        File.Copy(file, GetDestinationFileName(file), overwrite: true);
                    }
                });

                _snackbarService.EnqueueWithOk(GetDoneMessage(files.Count, validFileCount));
                message.DragEventArgs.Handled = true;
            }).ContinueWith((t) =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    Mouse.OverrideCursor = null;
                });
            });
        }

        private string GetDoneMessage(int fileCount, int validFileCount)
        {
            if (fileCount == 0)
            {
                return Properties.Resources.COULD_NOT_READ_FILE;
            }

            if (validFileCount == 0)
            {
                if (fileCount == 1)
                {
                    return Properties.Resources.COULD_NOT_READ_FILE;
                }

                return Properties.Resources.COULD_NOT_READ_ANY;
            }

            if (validFileCount < fileCount)
            {
                int badFiles = fileCount - validFileCount;

                if (badFiles == 1)
                {
                    return Properties.Resources.COULD_NOT_READ_1;
                }

                return string.Format(Properties.Resources.COULD_NOT_READ_X, badFiles);
            }

            if (validFileCount == 1)
            {
                return Properties.Resources.ADDED_FILE;
            }

            return string.Format(Properties.Resources.ADDED_X_FILES, validFileCount);
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
