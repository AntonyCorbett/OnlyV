using System.IO;

namespace OnlyVThemeCreator.AppOptions
{
    internal class Options
    {
        public string AppWindowPlacement { get; set; }

        public string EpubPath { get; set; }

        public string Culture { get; set; }

        public void Sanitize()
        {
            if (!File.Exists(EpubPath))
            {
                EpubPath = null;
            }
        }
    }
}
