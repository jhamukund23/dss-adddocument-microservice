using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace dss_adddocument_microservice.services
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// Base class for implementing Kafka Consumer.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue> where TValue : class
    {
        private readonly ConsumerConfig _config;
        private IKafkaHandler<TKey, TValue> _handler;
        private IConsumer<TKey, TValue> _consumer;
        private string _topic;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<KafkaConsumer<TKey, TValue>> _logger;
        /// <summary>
        /// Indicates constructor to initialize the serviceScopeFactory and ConsumerConfig
        /// </summary>
        /// <param name="config">Indicates the consumer configuration</param>
        /// <param name="serviceScopeFactory">Indicates the instance for serviceScopeFactory</param>
        public KafkaConsumer(ConsumerConfig config,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<KafkaConsumer<TKey, TValue>> logger
            )
        {
            _serviceScopeFactory = serviceScopeFactory;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Triggered when the service is ready to consume the Kafka topic.
        /// </summary>
        /// <param name="topic">Indicates Kafka Topic</param>
        /// <param name="stoppingToken">Indicates stopping token</param>
        /// <returns></returns>
        public async Task Consume(string topic, CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            _handler = scope.ServiceProvider.GetRequiredService<IKafkaHandler<TKey, TValue>>();
            _consumer = new ConsumerBuilder<TKey, TValue>(_config).SetValueDeserializer(new KafkaDeserializer<TValue>()).Build();
            _topic = topic;

            await Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        /// <summary>
        /// This will close the consumer, commit offsets and leave the group cleanly.
        /// </summary>
        public void Close()
        {
            _consumer.Close();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the consumer
        /// </summary>
        public void DisposeKafkaConsumer()
        {
            _consumer.Dispose();
        }

        private async Task StartConsumerLoop(CancellationToken cancellationToken)
        {
            var timer = new Stopwatch();
            timer.Start();

            _consumer.Subscribe(_topic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(cancellationToken);

                    timer.Stop();
                    TimeSpan timeTaken = timer.Elapsed;
                    string totalTime = "Time taken: " + timeTaken.ToString(@"m\:ss\.fffff");
                    _logger.LogInformation("Consume message from kafka topic {0}", totalTime);

                    if (result.Message != null)
                    {
                        _logger.LogInformation("Kafka message consume from partition {0}, offset {1} and key {2}", result.Partition.Value, result.Offset, result.Message.Key);
                        await _handler.HandleAsync(result.Message.Key, result.Message.Value);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException e)
                {
                    // Consumer errors should generally be ignored (or logged) unless fatal.
                    _logger.LogError($"Consume error: {e.Error.Reason}");

                    if (e.Error.IsFatal)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"Unexpected error: {e}");
                    break;
                }
            }
        }
    }
}


