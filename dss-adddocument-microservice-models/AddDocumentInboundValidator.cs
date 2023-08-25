using FluentValidation;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
namespace dss_adddocument_microservice.models
{
    // AddDocumentInboundValidator validate AddDocumentInbound model.
    public class AddDocumentInboundValidator : AbstractValidator<AddDocumentInbound>
    {
        public AddDocumentInboundValidator()
        {
            RuleFor(x => x.Type).NotEmpty();
            RuleFor(x => x.Source).NotEmpty();
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Time).NotEmpty();
            RuleFor(x => x.Subject).NotEmpty();           
            // Below properties in Request child class.          
            _=RuleFor(x => x.Request.FileName).NotEmpty();
            _=RuleFor(x => x.Request.SysCode).NotEmpty();
            _=RuleFor(x => x.Request.Spn).NotEmpty();
        }
    }
}

