using AutoFixture;
using dss_adddocument_microservice.data;
using dss_adddocument_microservice.handler;
using dss_adddocument_microservice.models;
using dss_adddocument_microservice.services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace dss_adddocument_microservice.test.Handler
{
    [TestFixture]
    public class AddDocumentHandlerTest
    {
        private AddDocumentHandler _addDocumentHandler;
        private Mock<IKafkaProducer<string, AddDocumentOutbound>> _responseProducer;
        private Mock<IAzureStorage> _azureStorage;
        private Mock<IPostgresqlWrapper> _iPostgresqlWrapper;
        private Mock<IOptions<AppSetting>> _appSetting;
        private Mock<ILogger<AddDocumentHandler>> _logger;
        private Mock<IValidator<AddDocumentInbound>> _validator;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _responseProducer= new Mock<IKafkaProducer<string, AddDocumentOutbound>>();
            _azureStorage= new Mock<IAzureStorage>();
            _iPostgresqlWrapper= new Mock<IPostgresqlWrapper>();
            _appSetting = new Mock<IOptions<AppSetting>>();
            _logger = new Mock<ILogger<AddDocumentHandler>>();
            _validator= new Mock<IValidator<AddDocumentInbound>>();
            _fixture =new Fixture();
            _addDocumentHandler= new AddDocumentHandler(_responseProducer.Object, _azureStorage.Object, _iPostgresqlWrapper.Object, _appSetting.Object, _logger.Object, _validator.Object);
        }

        [Test]
        public void HandleAsync_When_Success()
        {
            var input = _fixture.Build<AddDocumentInbound>()
                                 .With(x => x.Id, Convert.ToString(Guid.NewGuid()))
                                 .Create();
            var addDocumentOutbound = _fixture.Build<AddDocumentOutbound>().Create();

            var outputResponse = _fixture.Build<OutputResponse>()
                                    .With(x => x.ErrorMsg, String.Empty).Without(x => x.ErrorCode)
                                    .Create();

            var sasUri = _fixture.Create<Uri>();
            var containerName = _fixture.Create<string>();
            var outBoundTopic = _fixture.Create<string>();
            
            AppSetting appSetting = new()
            {                             
                BLOB_CONTAINERNAME = containerName,
                KAFKA_TOPIC_OUTBOUND = outBoundTopic
            };

            //Mocking the IOptions to return appSetting.
            _appSetting.Setup(s => s.Value).Returns(appSetting);

            //Mocking the ValidateAsync method to return ValidationResult.        
            _validator.Setup(x => x.Validate(input)).Returns(new ValidationResult());

            //Mocking the AddRequest method to return expectedDbOutput in json format.
            _iPostgresqlWrapper.Setup(x => x.AddRequest(input)).Returns(outputResponse);

            //Mocking the GetUserDelegationSasDirectory method to return SAS Uri.
            _azureStorage.Setup(x => x.GetUserDelegationSasDirectory(It.IsAny<string>(), containerName, It.IsAny<string>())).Returns(sasUri);
            addDocumentOutbound.Response.Sas = sasUri.ToString();
            addDocumentOutbound.Response.DocId = outputResponse.DocumentId;
            addDocumentOutbound.Id = Convert.ToString(It.IsAny<Guid>());
            addDocumentOutbound.Response.RequestId = input.Id;
            //Mocking the ProduceAsync method.
            _responseProducer.Setup(x => x.ProduceAsync(outBoundTopic, It.IsAny<string>(), It.IsAny<AddDocumentOutbound>()));

            //Mocking the UpdateResponse method.
            _iPostgresqlWrapper.Setup(x => x.UpdateResponse(It.IsAny<AddDocumentOutbound>())).Returns(outputResponse);

            //Act
            _addDocumentHandler.HandleAsync(It.IsAny<string>(), input);

            //Verify        
            _responseProducer.Verify(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AddDocumentOutbound>()), Times.Once());           
            _azureStorage.Verify(x => x.GetUserDelegationSasDirectory(It.IsAny<string>(), containerName, It.IsAny<string>()), Times.Once());
            _iPostgresqlWrapper.Verify(x => x.AddRequest(input), Times.Once());
            _iPostgresqlWrapper.Verify(x => x.UpdateResponse(It.IsAny<AddDocumentOutbound>()), Times.Once());

        }
        
        [Test]
        public void HandleAsync_When_AddDocument_Return_1001()
        {
            var input = _fixture.Build<AddDocumentInbound>()
                                 .With(x => x.Id, Convert.ToString(Guid.NewGuid()))
                                 .Create();
            var addDocumentOutbound = _fixture.Build<AddDocumentOutbound>().Create();

            var outputResponse = _fixture.Build<OutputResponse>()
                                    .With(x => x.ErrorMsg, "Please check id and type are valid.").With(x => x.ErrorCode, 1001)
                                    .Create();           
            var containerName = _fixture.Create<string>();
            var outBoundTopic = _fixture.Create<string>();
            
            AppSetting appSetting = new()
            {               
                
                BLOB_CONTAINERNAME = containerName,
                KAFKA_TOPIC_OUTBOUND = outBoundTopic
            };            

            //Mocking the IOptions to return appSetting.
            _appSetting.Setup(s => s.Value).Returns(appSetting);

            //Mocking the ValidateAsync method to return ValidationResult.        
            _validator.Setup(x => x.Validate(input)).Returns(new ValidationResult());

            //Mocking the AddRequest method to return expectedDbOutput in json format.
            _iPostgresqlWrapper.Setup(x => x.AddRequest(input)).Returns(outputResponse);

            addDocumentOutbound.Response.Sas = null;
            addDocumentOutbound.Response.DocId = 0;
            addDocumentOutbound.Id = Convert.ToString(It.IsAny<Guid>());
            addDocumentOutbound.Response.RequestId = input.Id;
            //Mocking the ProduceAsync method.
            _responseProducer.Setup(x => x.ProduceAsync(outBoundTopic, It.IsAny<string>(), It.IsAny<AddDocumentOutbound>()));
            //Mocking the UpdateResponse method.
            _iPostgresqlWrapper.Setup(x => x.UpdateResponse(It.IsAny<AddDocumentOutbound>())).Returns(outputResponse);

            //Act
            _addDocumentHandler.HandleAsync(It.IsAny<string>(), input);

            //Verify                 
            _iPostgresqlWrapper.Verify(x => x.AddRequest(input), Times.Once());
            _responseProducer.Verify(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AddDocumentOutbound>()), Times.Once());
            _iPostgresqlWrapper.Verify(x => x.UpdateResponse(It.Is<AddDocumentOutbound>(x => x.Response.Error.ErrorCode==outputResponse.ErrorCode)), Times.Once());

        }

        [Test]
        public void HandleAsync_When_Request_Json_Not_Valid()
        {
            var input = _fixture.Build<AddDocumentInbound>()
                                 .With(x => x.Id, Convert.ToString(Guid.NewGuid()))
                                 .With(x => x.Source, string.Empty)
                                 .With(x => x.Type, string.Empty)
                                 .Create();
            var addDocumentOutbound = _fixture.Build<AddDocumentOutbound>().Create();

            var outputResponse = _fixture.Build<OutputResponse>()
                                    .With(x => x.ErrorMsg, "Please check the request for valid values.").With(x => x.ErrorCode, 412)
                                    .Create();
           
            var containerName = _fixture.Create<string>();
            var outBoundTopic = _fixture.Create<string>();
            
            AppSetting appSetting = new()
            {                            
                BLOB_CONTAINERNAME = containerName,
                KAFKA_TOPIC_OUTBOUND = outBoundTopic
            };

            //Mocking the IOptions to return appSetting.
            _appSetting.Setup(s => s.Value).Returns(appSetting);

            //Mocking the ValidateAsync method to return ValidationResult.        

            _validator.Setup(x => x.Validate(input))
                             .Returns(new ValidationResult(new List<ValidationFailure>()
                             {
                                 new ValidationFailure("Source","The Source should not be empty or null."){ErrorCode = "412"}
                             }));

            //Mocking the AddRequest method to return expectedDbOutput in json format.
            _iPostgresqlWrapper.Setup(x => x.AddRequest(input)).Returns(outputResponse);

            addDocumentOutbound.Response.Sas = null;
            addDocumentOutbound.Response.DocId = 0;
            addDocumentOutbound.Id = Convert.ToString(It.IsAny<Guid>());
            addDocumentOutbound.Response.RequestId = input.Id;
            //Mocking the ProduceAsync method.
            _responseProducer.Setup(x => x.ProduceAsync(outBoundTopic, It.IsAny<string>(), It.IsAny<AddDocumentOutbound>()));
            //Mocking the UpdateResponse method.
            _iPostgresqlWrapper.Setup(x => x.UpdateResponse(It.IsAny<AddDocumentOutbound>())).Returns(outputResponse);

            //Act
            _addDocumentHandler.HandleAsync(It.IsAny<string>(), input);

            //Verify                
            _responseProducer.Verify(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AddDocumentOutbound>()), Times.Once());
            _iPostgresqlWrapper.Verify(x => x.UpdateResponse(It.Is<AddDocumentOutbound>(x => x.Response.Error.ErrorCode==outputResponse.ErrorCode)), Times.Once());

        }  
    }
}

