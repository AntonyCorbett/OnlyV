using System.IO;
using System.Threading;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using OnlyVThemeCreator.Helpers;
using Serilog;

namespace OnlyVThemeCreator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "app is closing")]
    public partial class App : Application
    {
        private readonly string _appString = "OnlyVThemeCreator";
        private Mutex _appMutex;

        public App()
        {
            DispatcherHelper.Initialize();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _appMutex?.Dispose();
            Log.Logger.Information("==== Exit ====");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (AnotherInstanceRunning())
            {
                Shutdown();
            }
            else
            {
                ConfigureLogger();
            }
        }

        private static void ConfigureLogger()
        {
            string logsDirectory = FileUtils.GetLogFolder();

#if DEBUG
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(logsDirectory, "log-.txt"), retainedFileCountLimit: 28, rollingInterval: RollingInterval.Day)
                .CreateLogger();
#else
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(Path.Combine(logsDirectory, "log-.txt"), retainedFileCountLimit: 28, rollingInterval: RollingInterval.Day)
                .CreateLogger();
#endif

            Log.Logger.Information("==== Launched ====");
        }

        private bool AnotherInstanceRunning()
        {
            _appMutex = new Mutex(true, _appString, out var newInstance);
            return !newInstance;
        }
    }
}
