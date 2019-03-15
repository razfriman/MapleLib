using System;
using System.Runtime.Serialization;

namespace MapleLib.WzLib.Serialization
{
    public class NoBase64DataException : Exception
    {
        public NoBase64DataException()
        {
        }

        public NoBase64DataException(string message) : base(message)
        {
        }

        public NoBase64DataException(string message, Exception inner) : base(message, inner)
        {
        }

        protected NoBase64DataException(SerializationInfo info,
            StreamingContext context)
        {
        }
    }
}