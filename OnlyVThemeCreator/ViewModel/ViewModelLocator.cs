namespace OnlyVThemeCreator.ViewModel
{
    using CommonServiceLocator;
    using GalaSoft.MvvmLight.Ioc;
    using OnlyV.Themes.Common.Services.UI;
    using Services;

    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ShouldSaveViewModel>();

            SimpleIoc.Default.Register<IOptionsService, OptionsService>();
            SimpleIoc.Default.Register<IUserInterfaceService, UserInterfaceService>();
            SimpleIoc.Default.Register<IDialogService, DialogService>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        public ShouldSaveViewModel ShouldSaveDialog => ServiceLocator.Current.GetInstance<ShouldSaveViewModel>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}