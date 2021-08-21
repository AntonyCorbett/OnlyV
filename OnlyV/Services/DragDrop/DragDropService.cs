﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using OnlyV.Helpers;
using OnlyV.PubSubMessages;
using OnlyV.Services.Bible;
using OnlyV.Services.Snackbar;
using OnlyV.Themes.Common.Services.UI;
using Serilog;

namespace OnlyV.Services.DragDrop
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DragDropService : IDragDropService
    {
        private readonly IBibleVersesService _bibleVersesService;
        private readonly IUserInterfaceService _userInterfaceService;
        private readonly ISnackbarService _snackbarService;

        public DragDropService(
            IBibleVersesService bibleVersesService,
            IUserInterfaceService userInterfaceService,
            ISnackbarService snackbarService)
        {
            _bibleVersesService = bibleVersesService;
            _userInterfaceService = userInterfaceService;
            _snackbarService = snackbarService;

            Messenger.Default.Register<DragOverMessage>(this, OnDragOver);
            Messenger.Default.Register<DragDropMessage>(this, OnDragDrop);
        }

        public event EventHandler EpubFileListChanged;

        public bool CanAcceptDrop(DragEventArgs e)
        {
            if (e.Data is DataObject dataObject && dataObject.ContainsFileDropList())
            {
                foreach (var filePath in dataObject.GetFileDropList())
                {
                    if (IsEpubFile(filePath))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public IReadOnlyCollection<string> GetDroppedFiles(DragEventArgs e)
        {
            var result = new List<string>();

            if (e.Data is DataObject dataObject && dataObject.ContainsFileDropList())
            {
                foreach (var filePath in dataObject.GetFileDropList())
                {
                    if (IsEpubFile(filePath))
                    {
                        result.Add(filePath);
                    }
                }
            }

            return result;
        }

        private static bool IsEpubFile(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            return ext != null && ext.Equals(".epub", StringComparison.OrdinalIgnoreCase);
        }

        private void OnDragOver(DragOverMessage message)
        {
            message.DragEventArgs.Effects = CanAcceptDrop(message.DragEventArgs)
                ? DragDropEffects.Copy
                : DragDropEffects.None;

            message.DragEventArgs.Handled = true;
        }

        private void OnDragDrop(DragDropMessage message)
        {
            var epubFolder = FileUtils.GetEpubFolder();
            if (!FileUtils.DirectoryIsWritable(epubFolder))
            {
                message.DragEventArgs.Handled = true;
                _snackbarService.EnqueueWithOk(string.Format(Properties.Resources.NO_WRITE_ACCESS, epubFolder));
                return;
            }

            var busyCursor = _userInterfaceService.GetBusy();

            var origEpubPath = _bibleVersesService.EpubPath;

            Task.Run(() =>
            {
                var files = GetDroppedFiles(message.DragEventArgs);

                var validFileCount = 0;

                // close reader so that we can overwrite the current epub (if necessary)
                _bibleVersesService.CloseReader();

                Parallel.ForEach(files, file =>
                {
                    try
                    {
                        if (_bibleVersesService.IsValidBibleEpub(file))
                        {
                            File.Copy(file, GetDestinationFileName(file), overwrite: true);
                            Interlocked.Increment(ref validFileCount);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, $@"Could not copy epub file {file}");
                    }
                });

                _snackbarService.EnqueueWithOk(GetDoneMessage(files.Count, validFileCount));
                message.DragEventArgs.Handled = true;
            }).ContinueWith(t =>
            {
                _bibleVersesService.EpubPath = origEpubPath;

                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    EpubFileListChanged?.Invoke(this, EventArgs.Empty);
                    busyCursor.Dispose();
                });
            });
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

        private static string GetDoneMessage(int fileCount, int validFileCount)
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
    }
}
