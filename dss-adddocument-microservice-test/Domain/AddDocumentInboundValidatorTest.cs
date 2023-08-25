using AutoFixture;
using dss_adddocument_microservice.models;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;

namespace dss_adddocument_microservice_test.Domain
{
    [TestFixture]
    public class AddDocumentInboundValidatorTest
    {
        readonly AddDocumentInboundValidator _addDocumentInboundValidator = new();
        private Mock<IValidator<AddDocumentInbound>> _validator;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _validator= new Mock<IValidator<AddDocumentInbound>>();
            _fixture =new Fixture();
        }

        [Test]
        public void WhenAddDocumentInboundIsValidReturnSuccess()
        {
            var input = _fixture.Build<AddDocumentInbound>().Create();
            _validator.Setup(x => x.Validate(input)).Returns(new ValidationResult());
            var error = _addDocumentInboundValidator.Validate(input);
            Assert.IsTrue(error.IsValid);
        }

        [Test]
        public void WhenAddDocumentInboundSourceIsEmptyAndNull()
        {
            var input = _fixture.Build<AddDocumentInbound>()
                                 .With(x => x.Source, string.Empty)
                                 .Create();
            var error = _addDocumentInboundValidator.Validate(input);
            Assert.IsFalse(error.IsValid);
        }
        [Test]
        public void WhenAddDocumentInboundTypeIsEmptyAndNull()
        {
            var input = _fixture.Build<AddDocumentInbound>()
                                 .With(x => x.Type, string.Empty)
                                 .Create();
            var error = _addDocumentInboundValidator.Validate(input);
            Assert.IsFalse(error.IsValid);
        }
        [Test]
        public void WhenAddDocumentInboundIdIsEmptyAndNull()
        {
            var input = _fixture.Build<AddDocumentInbound>()
                                 .With(x => x.Id, string.Empty)
                                 .Create();
            var error = _addDocumentInboundValidator.Validate(input);
            Assert.IsFalse(error.IsValid);
        }
        [Test]
        public void WhenAddDocumentInboundTimeIsEmptyAndNull()
        {
            var input = _fixture.Build<AddDocumentInbound>()
                                 .With(x => x.Time, (DateTime?)null)
                                 .Create();
            var error = _addDocumentInboundValidator.Validate(input);
            Assert.IsFalse(error.IsValid);
        }
        [Test]
        public void WhenAddDocumentInboundSubjectIsEmptyAndNull()
        {
            var input = _fixture.Build<AddDocumentInbound>()
                                 .With(x => x.Subject, string.Empty)
                                 .Create();
            var error = _addDocumentInboundValidator.Validate(input);
            Assert.IsFalse(error.IsValid);
        }
    }
}

