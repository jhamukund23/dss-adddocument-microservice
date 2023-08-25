using FluentValidation;

namespace dss_adddocument_microservice.models
{
    // AddDocumentOutboundValidator validate AddDocumentOutbound model.
    public class AddDocumentOutboundValidator : AbstractValidator<AddDocumentOutbound>
    {
        public AddDocumentOutboundValidator()
        {
            RuleFor(x => x.Type).NotEmpty();
            RuleFor(x => x.Source).NotEmpty();
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Time).NotEmpty();
            RuleFor(x => x.Subject).NotEmpty();           
            // Below properties in Response child class.
            _=RuleFor(x => x.Response.DocId).NotEmpty();
            _=RuleFor(x => x.Response.Version).NotEmpty();
            _=RuleFor(x => x.Response.RequestId).NotEmpty();
            _=RuleFor(x => x.Response.DocId).NotEmpty();
            _=RuleFor(x => x.Response.Sas).NotEmpty();
        }
    }
}

