using Confluent.Kafka;
using dss_adddocument_microservice.services;
using System.Diagnostics.CodeAnalysis;

namespace dss_adddocument_microservice.services
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// Base class for implementing Kafka Producer.
    /// </summary>
    /// <typeparam name="TKey">Indicates message's key in Kafka topic</typeparam>
    /// <typeparam name="TValue">Indicates message's value in Kafka topic</typeparam>
    public class KafkaProducer<TKey, TValue> : IDisposable, IKafkaProducer<TKey, TValue> where TValue : class
    {
        private readonly IProducer<TKey, TValue> _producer;

        /// <summary>
        /// Initializes the producer
        /// </summary>
        /// <param name="config"></param>
        public KafkaProducer(ProducerConfig config)
        {
            _producer = new ProducerBuilder<TKey, TValue>(config).SetValueSerializer(new KafkaSerializer<TValue>()).Build();
        }

        /// <summary>
        /// Triggered when the service is ready to send a single message the Kafka topic.  
        /// </summary>
        /// <param name="topic">Indicates topic name</param>
        /// <param name="key">Indicates message's key in Kafka topic</param>
        /// <param name="value">Indicates message's value in Kafka topic</param>
        /// <returns></returns>
        public async Task ProduceAsync(string topic, TKey key, TValue value)
        {
            await _producer.ProduceAsync(topic, new Message<TKey, TValue> { Key = key, Value = value });

        }

        /// <summary>
        /// Triggered when the service is ready to send a single message the Kafka TopicPartition.  
        /// </summary>
        /// <param name="topicPartition">Indicates topic Partition</param>
        /// <param name="key">Indicates message's key in Kafka topic</param>
        /// <param name="value">Indicates message's value in Kafka topic</param>
        /// <returns></returns>
        public async Task ProduceAsync(TopicPartition topicPartition, TKey key, TValue value)
        {
            await _producer.ProduceAsync(topicPartition, new Message<TKey, TValue> { Key = key, Value = value });

        }

        /// <summary>
        /// Cleanup object reference
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);           
        }
        protected virtual void Dispose(bool disposing)
        {
            _producer.Flush();
            _producer.Dispose();
        }

    }
}
    

