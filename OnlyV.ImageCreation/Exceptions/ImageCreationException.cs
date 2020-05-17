namespace OnlyV.ImageCreation.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ImageCreationException : Exception
    {
        public ImageCreationException(string message)
            : base(message)
        {
        }

        public ImageCreationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        
        protected ImageCreationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
