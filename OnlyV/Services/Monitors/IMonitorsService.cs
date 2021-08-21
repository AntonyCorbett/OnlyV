using System;
using System.Collections.Generic;

namespace OnlyV.Services.Monitors
{
    public interface IMonitorsService
    {
        IEnumerable<SystemMonitor> GetSystemMonitors();

        SystemMonitor GetSystemMonitor(string monitorId);

        SystemMonitor GetMonitorForWindowHandle(IntPtr windowHandle);
    }
}
