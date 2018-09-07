namespace OnlyV.ViewModel
{
    using System.Windows.Media;
    using GalaSoft.MvvmLight;

    internal class DisplayViewModel : ViewModelBase
    {
        private ImageSource _image;

        public ImageSource ImageSource
        {
            get => _image;
            set
            {
                if (!Equals(value, _image))
                {
                    _image = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
