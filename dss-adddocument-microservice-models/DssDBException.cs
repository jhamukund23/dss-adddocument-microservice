using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace dss_adddocument_microservice.models
{
    //Custom exception which inherits base class Exception
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class DssDBException : Exception
    {
        public DssDBException() { }
        // Constructor with message argument allows overriding default error message.
        // Should be included if users can provide more helpful messages than
        // generic automatically generated messages.
        public DssDBException(string message) : base(message) { }
        public DssDBException(string message, Exception inner) : base(message, inner) { }
        // Constructor for serialization support. If your exception contains custom properties, read their values here.      
        protected DssDBException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    }
}
