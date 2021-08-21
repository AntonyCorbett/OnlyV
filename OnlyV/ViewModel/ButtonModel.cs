using GalaSoft.MvvmLight.CommandWpf;

namespace OnlyV.ViewModel
{
    internal class ButtonModel
    {
        public ButtonModel(string content, object cmdParam, RelayCommand<object> command)
        {
            Content = content;
            Command = command;
            CommandParameter = cmdParam;
        }

        public string Content { get; set; }

        public RelayCommand<object> Command { get; set; }

        public object CommandParameter { get; set; }

        public bool Selected { get; set; }
    }
}
