namespace OnlyV.ViewModel
{
    using CommonServiceLocator;
    using GalaSoft.MvvmLight.Ioc;
    using OnlyV.Services.LoggingLevel;
    using OnlyV.Services.VerseEditor;
    using OnlyV.Themes.Common.Services.UI;
    using Services.Bible;
    using Services.CommandLine;
    using Services.DisplayWindow;
    using Services.DragDrop;
    using Services.Images;
    using Services.Monitors;
    using Services.Options;
    using Services.Snackbar;

    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    internal class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<DisplayViewModel>();
            SimpleIoc.Default.Register<ScripturesViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<PreviewViewModel>();
            SimpleIoc.Default.Register<StartupViewModel>();
            SimpleIoc.Default.Register<EditTextViewModel>();

            SimpleIoc.Default.Register<ICommandLineService, CommandLineService>();
            SimpleIoc.Default.Register<IBibleVersesService, BibleVersesService>();
            SimpleIoc.Default.Register<IOptionsService, OptionsService>();
            SimpleIoc.Default.Register<IImagesService, ImagesService>();
            SimpleIoc.Default.Register<IMonitorsService, MonitorsService>();
            SimpleIoc.Default.Register<IDisplayWindowService, DisplayWindowService>();
            SimpleIoc.Default.Register<ISnackbarService, SnackbarService>();
            SimpleIoc.Default.Register<IDragDropService, DragDropService>();
            SimpleIoc.Default.Register<IUserInterfaceService, UserInterfaceService>();
            SimpleIoc.Default.Register<ILogLevelSwitchService, LogLevelSwitchService>();
            SimpleIoc.Default.Register<IVerseEditorService, VerseEditorService>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        public DisplayViewModel Display => ServiceLocator.Current.GetInstance<DisplayViewModel>();

        public ScripturesViewModel Scriptures => ServiceLocator.Current.GetInstance<ScripturesViewModel>();
        
        public SettingsViewModel Settings => ServiceLocator.Current.GetInstance<SettingsViewModel>();

        public PreviewViewModel Preview => ServiceLocator.Current.GetInstance<PreviewViewModel>();

        public StartupViewModel Startup => ServiceLocator.Current.GetInstance<StartupViewModel>();

        public EditTextViewModel EditText => ServiceLocator.Current.GetInstance<EditTextViewModel>();
    }
}