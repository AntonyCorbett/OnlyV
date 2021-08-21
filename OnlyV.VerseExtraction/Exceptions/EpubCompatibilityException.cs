using System;

namespace OnlyV.VerseExtraction.Exceptions
{
    [Serializable]
    public class EpubCompatibilityException : Exception
    {
        public EpubCompatibilityException()
            : base(Properties.Resources.EPUB_INCOMPATIBLE)
        {
        }

        public EpubCompatibilityException(string message) 
            : base(message)
        {
        }

        public EpubCompatibilityException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
