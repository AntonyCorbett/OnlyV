namespace OnlyV
{
    using System.IO;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;
    using GalaSoft.MvvmLight.Threading;
    using OnlyV.Helpers;
    using OnlyV.Mappings;
    using Serilog;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "The app is closing anyway")]
    public partial class App : Application
    {
        private readonly string _appString = "OnlyVBibleTextImageTool";
        private Mutex _appMutex;

        public App()
        {
            DispatcherHelper.Initialize();
            RegisterMappings();
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

        private void ConfigureLogger()
        {
            string logsDirectory = FileUtils.GetLogFolder();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.RollingFile(Path.Combine(logsDirectory, "log-{Date}.txt"), retainedFileCountLimit: 28)
                .CreateLogger();

            Log.Logger.Information("==== Launched ====");
        }

        private bool AnotherInstanceRunning()
        {
            _appMutex = new Mutex(true, _appString, out var newInstance);
            return !newInstance;
        }

        private void RegisterMappings()
        {
            AutoMapper.Mapper.Initialize(cfg => cfg.AddProfile<MappingProfile>());
        }
    }
}
