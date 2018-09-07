namespace OnlyV.Services.Monitors
{
    using System;
    using System.Collections.Generic;

    public interface IMonitorsService
    {
        IEnumerable<SystemMonitor> GetSystemMonitors();

        SystemMonitor GetSystemMonitor(string monitorId);

        SystemMonitor GetMonitorForWindowHandle(IntPtr windowHandle);
    }
}
