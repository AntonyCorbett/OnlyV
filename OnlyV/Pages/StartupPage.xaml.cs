using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using OnlyV.PubSubMessages;

namespace OnlyV.Pages
{
    /// <summary>
    /// Interaction logic for StartupPage.xaml
    /// </summary>
    public partial class StartupPage : UserControl
    {
        public StartupPage()
        {
            InitializeComponent();
        }

        private void OnBibleDragOver(object sender, DragEventArgs e)
        {
            Messenger.Default.Send(new DragOverMessage { DragEventArgs = e });
        }

        private void OnBibleDrop(object sender, DragEventArgs e)
        {
            Messenger.Default.Send(new DragDropMessage { DragEventArgs = e });
        }
    }
}
