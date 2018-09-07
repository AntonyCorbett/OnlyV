namespace OnlyV.Services.DisplayWindow
{
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Media;
    using GalaSoft.MvvmLight.Messaging;
    using OnlyV.Helpers.WindowPositioning;
    using OnlyV.PubSubMessages;
    using OnlyV.Services.Monitors;
    using OnlyV.Services.Options;
    using Serilog;

    internal class DisplayWindowService : IDisplayWindowService
    {
        private readonly IMonitorsService _monitorsService;
        private readonly IOptionsService _optionsService;
        private readonly (int dpiX, int dpiY) _systemDpi;
        private Windows.DisplayWindow _displayWindow;

        public DisplayWindowService(
            IMonitorsService monitorsService,
            IOptionsService optionsService)
        {
            _monitorsService = monitorsService;
            _optionsService = optionsService;

            _systemDpi = WindowPlacement.GetDpiSettings();

            Messenger.Default.Register<ShutDownMessage>(this, OnShutDown);
        }

        public bool IsWindowVisible => _displayWindow != null && _displayWindow.IsVisible;

        public void ShowWindow()
        {
            EnsureWindowCreated();
            _displayWindow.Show();
        }

        public void HideWindow()
        {
            _displayWindow?.Hide();
        }

        public void CloseWindow()
        {
            _displayWindow?.Close();
            _displayWindow = null;
        }

        public void ToggleWindow()
        {
            if (IsWindowVisible)
            {
                HideWindow();
            }
            else
            {
                ShowWindow();
            }
        }

        public void ClearImage()
        {
            if (_displayWindow != null)
            {
                _displayWindow.TheImage.Source = null;
            }
        }

        public void SetImage(ImageSource image)
        {
            if (_displayWindow != null)
            {
                _displayWindow.TheImage.Source = image;
            }
        }
        
        private void EnsureWindowCreated()
        {
            if (_displayWindow == null)
            {
                _displayWindow = new Windows.DisplayWindow();

                var targetMonitor = _monitorsService.GetSystemMonitor(_optionsService.Options.MediaMonitorId);
                if (targetMonitor != null)
                {
                    Log.Logger.Information("Opening display window");

                    LocateWindowAtOrigin(_displayWindow, targetMonitor.Monitor);

                    _displayWindow.Topmost = true;
                }
            }
        }

        private void LocateWindowAtOrigin(Window window, Screen monitor)
        {
            var area = monitor.WorkingArea;

            var left = (area.Left * 96) / _systemDpi.dpiX;
            var top = (area.Top * 96) / _systemDpi.dpiY;

            Log.Logger.Verbose($"Monitor = {monitor.DeviceName} Left = {left}, top = {top}");

            // these seemingly redundant sizing statements are required!
            window.Left = 0;
            window.Top = 0;
            window.Width = 0;
            window.Height = 0;

            window.Left = left;
            window.Top = top;
        }

        private void OnShutDown(ShutDownMessage msg)
        {
            CloseWindow();
        }
    }
}
