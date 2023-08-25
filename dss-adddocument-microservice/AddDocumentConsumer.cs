using dss_adddocument_microservice.models;
using dss_adddocument_microservice.services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace dss_adddocument_microservice
{
    [ExcludeFromCodeCoverage]
    public class AddDocumentConsumer : BackgroundService
    {
        // Inject IKafkaConsumer,IOptions services inside constructor.
        private readonly IKafkaConsumer<string, AddDocumentInbound> _consumer;
        private readonly IOptions<AppSetting> _appSetting;
        public AddDocumentConsumer(
            IKafkaConsumer<string, AddDocumentInbound> kafkaConsumer,
            IOptions<AppSetting> appSetting
            )
        {
            _consumer = kafkaConsumer;
            _appSetting = appSetting;
        }

        // Call ExecuteAsync() Method when any new message comes in kafka topic.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {               
                // Checking kafka inbound topic is null or not.
                if (!string.IsNullOrEmpty(_appSetting.Value.KAFKA_TOPIC_INBOUND))
                {
                    // Calling consume method for read kafka inbound message.
                    await _consumer.Consume(_appSetting.Value.KAFKA_TOPIC_INBOUND, stoppingToken);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"ConsumeFailedOnTopic - {_appSetting?.Value.KAFKA_TOPIC_INBOUND}, {ex}");
            }
        }

        // Removed unused references
        public override void Dispose()
        {
            _consumer.Close();
            GC.SuppressFinalize(this);
            base.Dispose();
        }

    }
}
