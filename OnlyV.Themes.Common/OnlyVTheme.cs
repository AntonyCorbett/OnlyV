namespace OnlyV.Themes.Common
{
    // ReSharper disable MemberCanBePrivate.Global
    using Specs;

    public class OnlyVTheme
    {
        public OnlyVTheme()
        {
            Dimensions = new OnlyVDimensions();
            Background = new OnlyVBackground();
            BodyText = new OnlyVBodyTextSpec();
            TitleText = new OnlyVTitleTextSpec();
            VerseNumbers = new OnlyVVerseNumberSpec();
        }

        public OnlyVDimensions Dimensions { get; set; }

        public OnlyVBackground Background { get; set; }
        
        public OnlyVBodyTextSpec BodyText { get; set; }

        public OnlyVTitleTextSpec TitleText { get; set; }

        public OnlyVVerseNumberSpec VerseNumbers { get; set; }
    }
}
