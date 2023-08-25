using AutoFixture;
using dss_adddocument_microservice;
using dss_adddocument_microservice.models;
using dss_adddocument_microservice.services;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace dss_adddocument_microservice_test
{
    [TestFixture]
    public class AddDocumentConsumerTest
    {
        private AddDocumentConsumer _addDocumentConsumer;
        private Mock<IKafkaConsumer<string, AddDocumentInbound>> _consumer;
        private Mock<IOptions<AppSetting>> _appSetting;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _consumer= new Mock<IKafkaConsumer<string, AddDocumentInbound>>();
            _appSetting = new Mock<IOptions<AppSetting>>();
            _fixture =new Fixture();
            _addDocumentConsumer= new AddDocumentConsumer(_consumer.Object, _appSetting.Object);
        }

        [Test]
        public void ExecuteAsync()
        {
            var stoppingToken = new CancellationToken();
            var kafkaTopic = _fixture.Create<string>();
            AppSetting appSetting = new() { KAFKA_TOPIC_INBOUND = kafkaTopic };
            _appSetting.Setup(s => s.Value).Returns(appSetting);

            _consumer.Setup(x => x.Consume(kafkaTopic, stoppingToken))
               .Returns(Task.CompletedTask);           
            _consumer.Verify(x => x.Consume(kafkaTopic, stoppingToken), Times.Never);

        }

        [Test]
        public void Dispose()
        {
            _addDocumentConsumer.Dispose();
            _consumer.Verify(x => x.Close(), Times.Once);            
        }
    }
}
