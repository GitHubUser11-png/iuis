using System;
using System.Runtime.Serialization;

namespace IUIS.Domain.Common
{
    [Serializable]
    public sealed class DomainValidationException : Exception
    {
        public DomainValidationException()
        {
        }

        public DomainValidationException(string message)
            : base(message)
        {
        }

        public DomainValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        private DomainValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
