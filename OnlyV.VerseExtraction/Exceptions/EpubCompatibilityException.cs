namespace OnlyV.VerseExtraction.Exceptions
{
    using System;

    [Serializable]
    public class EpubCompatibilityException : Exception
    {
        public EpubCompatibilityException()
            : base(Properties.Resources.EPUB_INCOMPATIBLE)
        {
        }
    }
}
