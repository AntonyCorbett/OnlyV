namespace OnlyV.Services.DisplayWindow
{
    using System.Windows;
    using System.Windows.Forms;
    using OnlyV.Helpers.WindowPositioning;
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
        }

        public void OpenWindow()
        {
            EnsureWindowCreated();
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

                    _displayWindow.Show();
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
    }
}
