namespace OnlyV.Services.UI
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class UserInterfaceService : IUserInterfaceService
    {
        public BusyCursor GetBusy()
        {
            return new BusyCursor();
        }
    }
}
