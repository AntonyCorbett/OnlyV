using System.Windows.Media;

namespace OnlyV.Services.DisplayWindow
{
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
