namespace OnlyV.Themes.Common.Specs
{
    // ReSharper disable MemberCanBePrivate.Global
    public class OnlyVBodyTextSpec
    {
        public OnlyVBodyTextSpec()
        {
            Font = new OnlyVFontSpec
            {
                Colour = "#fbfbff",
                Size = 86
            };

            HorizontalAlignment = OnlyVHorizontalTextAlignment.Centre;
            LineSpacing = OnlyVLineSpacing.Normal;

            DropShadow = new OnlyVDropShadowSpec { Show = true };
        }

        public OnlyVFontSpec Font { get; set; }

        public OnlyVHorizontalTextAlignment HorizontalAlignment { get; set; }

        public OnlyVLineSpacing LineSpacing { get; set; }

        public OnlyVDropShadowSpec DropShadow { get; set; }
    }
}
