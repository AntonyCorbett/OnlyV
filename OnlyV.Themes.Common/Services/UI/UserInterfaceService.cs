namespace OnlyV.Themes.Common.Services.UI
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UserInterfaceService : IUserInterfaceService
    {
        public BusyCursor GetBusy()
        {
            return new BusyCursor();
        }
    }
}
