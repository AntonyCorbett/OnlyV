using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using OnlyV.PubSubMessages;

namespace OnlyV.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
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
