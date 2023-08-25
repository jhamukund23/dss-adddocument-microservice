using AutoFixture;
using dss_adddocument_microservice.models;
using NUnit.Framework;

namespace dss_adddocument_microservice_test.Domain
{
    [TestFixture]
    public class AddDocumentOutboundValidatorTest
    {
        readonly AddDocumentOutboundValidator _addDocumentOutboundValidator;
        public AddDocumentOutboundValidatorTest()
        {
            _addDocumentOutboundValidator = new AddDocumentOutboundValidator();
        }
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture =new Fixture();
        }

        [Test]
        public void WhenAddDocumentOutboundIsValidReturnSuccess()
        {
            var input = _fixture.Build<AddDocumentOutbound>().Create();
            var error = _addDocumentOutboundValidator.Validate(input);
            Assert.IsTrue(error.IsValid);
        }


        [Test]
        public void WhenAddDocumentOutboundIsInvalidReturnErrors()
        {
            var input = _fixture
                    .Build<AddDocumentOutbound>()
                    .Without(b => b.Response)
                    .Do(b =>
                    {
                        b.Response = _fixture.Create<Response>();
                    })
                    .Create();
            var error = _addDocumentOutboundValidator.Validate(input);
            Assert.IsTrue(error.IsValid);
        }
    }
}

