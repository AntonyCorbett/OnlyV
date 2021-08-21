using System.Windows;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Messaging;
using OnlyV.Themes.Common.Services.WindowPositioning;
using OnlyVThemeCreator.PubSubMessages;
using OnlyVThemeCreator.Services;

namespace OnlyVThemeCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<CloseAppMessage>(this, OnCloseAppMessage);
        }

        protected override void OnSourceInitialized(System.EventArgs e)
        {
            AdjustMainWindowPositionAndSize();
        }

        private void AdjustMainWindowPositionAndSize()
        {
            var optionsService = ServiceLocator.Current.GetInstance<IOptionsService>();
            if (!string.IsNullOrEmpty(optionsService.AppWindowPlacement))
            {
                ResizeMode = WindowState == WindowState.Maximized
                    ? ResizeMode.NoResize
                    : ResizeMode.CanResizeWithGrip;

                this.SetPlacement(optionsService.AppWindowPlacement);
            }
        }

        private void OnImageDragOver(object sender, DragEventArgs e)
        {
            Messenger.Default.Send(new DragOverMessage { DragEventArgs = e });
        }

        private void OnImageDrop(object sender, DragEventArgs e)
        {
            Messenger.Default.Send(new DragDropMessage { DragEventArgs = e });
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveOptions();
        }

        private void SaveOptions()
        {
            var optionsService = ServiceLocator.Current.GetInstance<IOptionsService>();
            optionsService.AppWindowPlacement = this.GetPlacement();
            optionsService.Save();
        }

        private void OnCloseAppMessage(CloseAppMessage obj)
        {
            Close();
        }
    }
}
