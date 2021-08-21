using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace OnlyV.Helpers
{
    internal static class BitmapHelper
    {
        public static BitmapImage ConvertBitmap(Bitmap source)
        {
            var ms = new MemoryStream();
            {
                source.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                var image = new BitmapImage();
                image.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
    }
}
