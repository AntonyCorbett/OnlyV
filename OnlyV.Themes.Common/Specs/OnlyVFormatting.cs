namespace OnlyV.Themes.Common.Specs
{
    // ReSharper disable MemberCanBePrivate.Global
    public class OnlyVFormatting
    {
        public OnlyVFormatting()
        {
            AutoFit = true;
            ShowVerseBreaks = true;
            UseTildeMarker = false;
            TrimPunctuation = true;
            TrimQuotes = true;
        }

        public bool AutoFit { get; set; }
        
        public bool ShowVerseBreaks { get; set; }

        public bool UseTildeMarker { get; set; }

        public bool TrimPunctuation { get; set; }

        public bool TrimQuotes { get; set; }
    }
}
