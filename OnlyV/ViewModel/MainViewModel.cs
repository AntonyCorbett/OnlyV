namespace OnlyV.ViewModel
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using MaterialDesignThemes.Wpf;
    using OnlyV.Services.Snackbar;
    using Services.Images;
    using Services.Options;

    // ReSharper disable once UnusedMember.Global
    internal class MainViewModel : ViewModelBase
    {
        private readonly ScripturesViewModel _scripturesViewModel;
        private readonly PreviewViewModel _previewViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        
        private readonly IImagesService _imagesService;
        private readonly IOptionsService _optionsService;
        private readonly ISnackbarService _snackbarService;

        private ViewModelBase _currentPage;
        private ViewModelBase _preSettingsPage;
        private string _nextPageTooltip;
        private string _previousPageTooltip;

        public MainViewModel(
            ScripturesViewModel scripturesViewModel,
            PreviewViewModel previewViewModel,
            SettingsViewModel settingsViewModel,
            IImagesService imagesService,
            IOptionsService optionsService,
            ISnackbarService snackbarService)
        {
            _scripturesViewModel = scripturesViewModel;
            _previewViewModel = previewViewModel;
            _settingsViewModel = settingsViewModel;

            _imagesService = imagesService;
            _optionsService = optionsService;
            _snackbarService = snackbarService;

            InitCommands();

            _currentPage = scripturesViewModel;
            _preSettingsPage = scripturesViewModel;

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

        public string SettingsButtonToolTip =>
            CurrentPage == _settingsViewModel
                ? Properties.Resources.BACK
                : Properties.Resources.SETTINGS_PAGE;

        public string SettingsIconKind => 
            CurrentPage == _settingsViewModel
                ? @"BackBurger"
                : @"Settings";

        public RelayCommand NextPageCommand { get; set; }

        public RelayCommand BackPageCommand { get; set; }

        public RelayCommand SettingsCommand { get; set; }

        public ISnackbarMessageQueue TheSnackbarMessageQueue => _snackbarService.TheSnackbarMessageQueue;

        private void InitCommands()
        {
            NextPageCommand = new RelayCommand(OnNext, CanDoNext);
            BackPageCommand = new RelayCommand(OnBack, CanDoBack);
            SettingsCommand = new RelayCommand(OnSettings, CanShowSettings);
        }

        private bool CanShowSettings()
        {
            return true;
        }

        private void OnSettings()
        {
            if (CurrentPage == _settingsViewModel)
            {
                CurrentPage = _preSettingsPage;
            }
            else
            {
                _preSettingsPage = CurrentPage;
                CurrentPage = _settingsViewModel;
            }

            RaisePropertyChanged(nameof(SettingsIconKind));
            RaisePropertyChanged(nameof(SettingsButtonToolTip));
        }

        private bool CanDoBack()
        {
            return CurrentPage == _previewViewModel;
        }

        private void OnBack()
        {
            if (CurrentPage == _previewViewModel)
            {
                CurrentPage = _scripturesViewModel;
            }
        }

        private bool CanDoNext()
        {
            if (CurrentPage == _scripturesViewModel)
            {
                return _scripturesViewModel.ValidScripture();
            }

            return false;
        }

        private void OnNext()
        {
            if (CurrentPage == _scripturesViewModel)
            {
                _previewViewModel.ImageIndex = null;
                InitImagesService();
                _previewViewModel.ImageIndex = 0;
                _previewViewModel.BookChapterAndVersesString = _scripturesViewModel.ScriptureText;

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