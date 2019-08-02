namespace OnlyV.ViewModel
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;

    internal class VerseButtonModel : ViewModelBase
    {
        private bool _selected;

        public VerseButtonModel(string content, object cmdParam, RelayCommand<object> command)
        {
            Content = content;
            Command = command;
            CommandParameter = cmdParam;
        }

        public string Content { get; set; }

        public RelayCommand<object> Command { get; set; }

        public object CommandParameter { get; set; }

        public bool Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
