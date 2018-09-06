namespace OnlyV.ViewModel
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using OnlyV.Services.Images;
    using OnlyV.Services.Options;

    internal class MainViewModel : ViewModelBase
    {
        private readonly ScripturesViewModel _scripturesViewModel;
        private readonly PreviewViewModel _previewViewModel;
        private readonly SettingsViewModel _settingsViewModel;

        private readonly IImagesService _imagesService;
        private readonly IOptionsService _optionsService;

        private ViewModelBase _currentPage;
        private string _nextPageTooltip;
        private string _previousPageTooltip;

        public MainViewModel(
            ScripturesViewModel scripturesViewModel,
            PreviewViewModel previewViewModel,
            SettingsViewModel settingsViewModel,
            IImagesService imagesService,
            IOptionsService optionsService)
        {
            _scripturesViewModel = scripturesViewModel;
            _previewViewModel = previewViewModel;
            _settingsViewModel = settingsViewModel;

            _imagesService = imagesService;
            _optionsService = optionsService;

            InitCommands();

            _currentPage = scripturesViewModel;
            
            _nextPageTooltip = Properties.Resources.NEXT_PAGE_PREVIEW;
        }

        public ViewModelBase CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    RaisePropertyChanged();

                    PreviousPageToolTip = 
                        _currentPage == _previewViewModel || _currentPage == _settingsViewModel
                            ? Properties.Resources.PREV_PAGE_SCRIPS
                            : null;

                    NextPageToolTip =
                        _currentPage == _scripturesViewModel || _currentPage == _settingsViewModel
                            ? Properties.Resources.NEXT_PAGE_PREVIEW
                            : null;
                }
            }
        }

        public string NextPageToolTip
        {
            get => _nextPageTooltip;
            set
            {
                if (_nextPageTooltip == null || _nextPageTooltip != value)
                {
                    _nextPageTooltip = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string PreviousPageToolTip
        {
            get => _previousPageTooltip;
            set
            {
                if (_previousPageTooltip == null || _previousPageTooltip != value)
                {
                    _previousPageTooltip = value;
                    RaisePropertyChanged();
                }
            }
        }

        public RelayCommand NextPageCommand { get; set; }

        public RelayCommand BackPageCommand { get; set; }

        public RelayCommand SettingsCommand { get; set; }

        private void InitCommands()
        {
            NextPageCommand = new RelayCommand(OnNext, CanDoNext);
            BackPageCommand = new RelayCommand(OnBack, CanDoBack);
            SettingsCommand = new RelayCommand(OnSettings, CanShowSettings);
        }

        private bool CanShowSettings()
        {
            return CurrentPage != _settingsViewModel;
        }

        private void OnSettings()
        {
            CurrentPage = _settingsViewModel;
        }

        private bool CanDoBack()
        {
            return CurrentPage != _scripturesViewModel;
        }

        private void OnBack()
        {
            if (CurrentPage == _previewViewModel || CurrentPage == _settingsViewModel)
            {
                CurrentPage = _scripturesViewModel;
            }
        }

        private bool CanDoNext()
        {
            if (CurrentPage == _scripturesViewModel || CurrentPage == _settingsViewModel)
            {
                return _scripturesViewModel.ValidScripture();
            }

            if (CurrentPage == _previewViewModel)
            {
                return false;
            }

            return false;
        }

        private void OnNext()
        {
            if (CurrentPage == _scripturesViewModel || CurrentPage == _settingsViewModel)
            {
                _previewViewModel.ImageIndex = null;
                InitImagesService();
                _previewViewModel.ImageIndex = 0;

                CurrentPage = _previewViewModel;
            }
        }

        private void InitImagesService()
        {
            _imagesService.Init(
                _optionsService.Options.EpubPath, 
                _scripturesViewModel.BookNumber, 
                _scripturesViewModel.ChapterAndVersesString);
        }
    }
}