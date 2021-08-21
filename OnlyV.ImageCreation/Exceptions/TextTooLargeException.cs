using System;
using System.Runtime.Serialization;

namespace OnlyV.ImageCreation.Exceptions
{
    [Serializable]
    public class TextTooLargeException : ImageCreationException
    {
        public TextTooLargeException()
            : base()
        {
        }

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
