namespace OnlyV.Services.Options
{
    internal interface IOptionsService
    {
        AppOptions.Options Options { get; }

        void Save();
    }
}
