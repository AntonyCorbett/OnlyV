namespace OnlyV.Services.DisplayWindow
{
    internal interface IDisplayWindowService
    {
        void ShowWindow();

        void HideWindow();

        void ToggleWindow();

        void CloseWindow();

        bool IsWindowShowing { get; }
    }
}
