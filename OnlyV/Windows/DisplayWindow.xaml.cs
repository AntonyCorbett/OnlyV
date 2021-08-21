using System.Windows;

namespace OnlyV.Windows
{
    public partial class DisplayWindow : Window
    {
        public DisplayWindow()
        {
            InitializeComponent();
        }

        private void OnDisplayWindowLoaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }
    }
}
