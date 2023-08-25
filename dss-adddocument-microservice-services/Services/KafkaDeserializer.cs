using Confluent.Kafka;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace dss_adddocument_microservice.services
{
    //Deserialize a message key or value.
    [ExcludeFromCodeCoverage]
    public sealed class KafkaDeserializer<T> : IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (typeof(T) == typeof(Null))
            {
                if (data.Length > 0)
                {
                    throw new ArgumentException("The data is null.");
                }
                return default;
            }
            if (typeof(T) == typeof(Ignore))
            {
                return default;
            }
            var dataJson = Encoding.UTF8.GetString(data);
            // Deserialize datajson in dynamic type DeserializeObject<T>
            return JsonConvert.DeserializeObject<T>(dataJson);
        }
    }
}
