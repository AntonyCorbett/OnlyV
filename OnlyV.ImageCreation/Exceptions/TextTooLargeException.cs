namespace OnlyV.ImageCreation.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class TextTooLargeException : ImageCreationException
    {
        public TextTooLargeException(string message)
            : base(message)
        {
        }

        public TextTooLargeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected TextTooLargeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
