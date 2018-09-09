namespace OnlyV.Services.DisplayWindow
{
    using System.Windows.Media;

    internal interface IDisplayWindowService
    {
        bool IsWindowVisible { get; }

        void ShowWindow();
        
        void ToggleWindow();

        void CloseWindow();

        void SetImage(ImageSource image);

        void ClearImage();

        void ChangeTargetMonitor();
    }
}
