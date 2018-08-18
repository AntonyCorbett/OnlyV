namespace OnlyV.VerseExtraction.Exceptions
{
    using System;

    [Serializable]
    public class EpubCompatibilityException : Exception
    {
        public EpubCompatibilityException()
            : base("The epub is not compatible with OnlyV")
        {
        }
    }
}
