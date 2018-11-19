namespace OnlyV.Pages
{
    using System.Windows.Controls;
    using System.Windows.Input;
    using OnlyV.ViewModel;

    /// <summary>
    /// Interaction logic for EditTextPage.xaml
    /// </summary>
    public partial class EditTextPage : UserControl
    {
        public EditTextPage()
        {
            InitializeComponent();
        }

        private void HandleGotKeyboardFocus(
            object sender, 
            KeyboardFocusChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            if (tb != null)
            {
                var vm = (EditVerseTextViewModel)tb.DataContext;
                vm.IsFocused = true;
            }
        }

        private void HandleLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            if (tb != null)
            {
                var vm = (EditVerseTextViewModel)tb.DataContext;
                vm.IsFocused = false;
            }
        }

        private void OpeningQuoteButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            InsertString("“");
        }

        private void ClosingQuoteButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            InsertString("”");
        }

        private void InsertString(string text)
        {
            var target = Keyboard.FocusedElement;
            var routedEvent = TextCompositionManager.TextInputEvent;

            target.RaiseEvent(
                new TextCompositionEventArgs(
                        InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, target, text))
                    { RoutedEvent = routedEvent });
        }
    }
}
