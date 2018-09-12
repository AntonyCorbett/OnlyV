namespace OnlyV.Themes.Common.Specs
{
    // ReSharper disable MemberCanBePrivate.Global
    public class OnlyVTitleTextSpec
    {
        public OnlyVTitleTextSpec()
        {
            Font = new OnlyVFontSpec
            {
                Colour = "#f0e100",
                Size = 64
            };

            HorizontalAlignment = OnlyVHorizontalTextAlignment.Right;
            Position = OnlyVTitlePosition.Bottom;
            DropShadow = new OnlyVDropShadowSpec { Show = true };
        }

        public OnlyVFontSpec Font { get; set; }

        public OnlyVHorizontalTextAlignment HorizontalAlignment { get; set; }

        public OnlyVTitlePosition Position { get; set; }

        public OnlyVDropShadowSpec DropShadow { get; set; }
    }
}
