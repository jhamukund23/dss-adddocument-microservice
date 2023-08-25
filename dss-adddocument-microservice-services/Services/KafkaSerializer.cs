using Confluent.Kafka;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace dss_adddocument_microservice.services
{
    // Serialize the key or value of a Confluent.Kafka.Message instance
    [ExcludeFromCodeCoverage]
    public sealed class KafkaSerializer<T> : ISerializer<T>
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            if (typeof(T) == typeof(Null))
            {
                return Array.Empty<byte>();
            }
            if (typeof(T) == typeof(Ignore))
            {
                throw new NotSupportedException("Not Supported.");
            }
            // Serialize data
            var json = JsonConvert.SerializeObject(data);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
