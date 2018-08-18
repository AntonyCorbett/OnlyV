namespace OnlyV.VerseExtraction.Models
{
    /// <summary>
    /// The Bible epubs come in 2 styles (which don't necessarily
    /// correspond to the rbi8 / nwt Bible types)
    /// </summary>
    internal enum EpubStyle
    {
        /// <summary>
        /// Style is not known.
        /// </summary>
        Unknown,

        /// <summary>
        /// An old style.
        /// </summary>
        Old,

        /// <summary>
        /// A new style.
        /// </summary>
        New
    }
}
