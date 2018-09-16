namespace OnlyVThemeCreator.AppOptions
{
    using System.IO;

    internal class Options
    {
        public string AppWindowPlacement { get; set; }

        public string EpubPath { get; set; }

        public void Sanitize()
        {
            if (!File.Exists(EpubPath))
            {
                EpubPath = null;
            }
        }
    }
}
