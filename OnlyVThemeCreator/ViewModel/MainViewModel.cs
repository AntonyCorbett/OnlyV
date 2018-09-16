namespace OnlyVThemeCreator.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Media;
    using GalaSoft.MvvmLight;
    using OnlyV.Themes.Common.Models;
    using OnlyV.Themes.Common.Services.UI;
    using OnlyV.VerseExtraction;
    using OnlyVThemeCreator.Helpers;
    using OnlyVThemeCreator.Services;

    public class MainViewModel : ViewModelBase
    {
        private readonly IUserInterfaceService _userInterfaceService;
        private readonly IOptionsService _optionsService;
        private int _currentSampleTextId;

        public MainViewModel(
            IUserInterfaceService userInterfaceService,
            IOptionsService optionsService)
        {
            _userInterfaceService = userInterfaceService;
            _optionsService = optionsService;

            BibleEpubFiles = GetBibleEpubFiles();
        }

        public event EventHandler EpubChangedEvent;

        public ImageSource ImageSource { get; set; }

        public IEnumerable<EpubFileItem> BibleEpubFiles { get; }

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
                        UpdateTextSamples();
                        EpubChangedEvent?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        public int CurrentSampleTextId
        {
            get => _currentSampleTextId;
            set
            {
                if (_currentSampleTextId != value)
                {
                    _currentSampleTextId = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<VerseTextItem> TextSamples { get; } = new ObservableCollection<VerseTextItem>();

        private void UpdateTextSamples()
        {
            var originalSampleId = CurrentSampleTextId;
            CurrentSampleTextId = 0;

            TextSamples.Clear();

            var newSamples = CreateTextSampleItems();
            foreach (var sample in CreateTextSampleItems())
            {
                TextSamples.Add(sample);
            }

            if (TextSamples.Any(x => x.Id.Equals(originalSampleId)))
            {
                CurrentSampleTextId = originalSampleId;
            }
            else
            {
                CurrentSampleTextId = TextSamples.FirstOrDefault()?.Id ?? 0;
            }
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

        private IReadOnlyCollection<VerseTextItem> CreateTextSampleItems()
        {
            var result = new List<VerseTextItem>();

            if (!string.IsNullOrEmpty(CurrentEpubFilePath) && File.Exists(CurrentEpubFilePath))
            {
                using (var reader = new BibleTextReader(CurrentEpubFilePath))
                {
                    foreach (var sample in StandardTextSample.GetStandardList())
                    {
                        result.Add(new VerseTextItem(
                            sample.Id,
                            GetSampleName(reader, sample), 
                            sample.BookNumber,
                            sample.ChapterAndVerses));
                    }
                }
            }

            return result;
        }

        private string GetSampleName(BibleTextReader reader, StandardTextSample sample)
        {
            return reader.GenerateVerseTitle(sample.BookNumber, sample.ChapterAndVerses);
        }
    }
}