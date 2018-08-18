namespace OnlyV.ViewModel
{
    using GalaSoft.MvvmLight;

    internal class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentPage;

        public MainViewModel(ScripturesViewModel scripturesViewModel)
        {
            CurrentPage = scripturesViewModel;
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
                }
            }
        }
    }
}