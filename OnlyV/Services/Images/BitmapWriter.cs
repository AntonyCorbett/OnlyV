namespace OnlyV.Services.Images
{
    using System.IO;
    using System.Windows.Media.Imaging;

    public static class BitmapWriter
    {
        public static void WriteJpeg(string fileName, int quality, BitmapSource bmp)
        {
            var encoder = new JpegBitmapEncoder();
            var outputFrame = BitmapFrame.Create(bmp);
            encoder.Frames.Add(outputFrame);
            encoder.QualityLevel = quality;

            using (var file = File.OpenWrite(fileName))
            {
                encoder.Save(file);
            }
        }

        public static void WritePng(string fileName, BitmapSource bmp)
        {
            var encoder = new PngBitmapEncoder();
            var outputFrame = BitmapFrame.Create(bmp);
            encoder.Frames.Add(outputFrame);

            using (var file = File.OpenWrite(fileName))
            {
                encoder.Save(file);
            }
        }
    }
}
