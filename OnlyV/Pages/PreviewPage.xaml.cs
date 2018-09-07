namespace OnlyV.Pages
{
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for PreviewPage.xaml
    /// </summary>
    public partial class PreviewPage : UserControl
    {
        public PreviewPage()
        {
            InitializeComponent();

            // for keyboard binding (e.g. F5)
            Focusable = true;
            Loaded += (s, e) => Keyboard.Focus(this);
        }
    }
}
