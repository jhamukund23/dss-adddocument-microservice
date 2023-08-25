using dss_adddocument_microservice.models;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
namespace dss_adddocument_microservice.data
{   
    // An interface representing postgresql database actions that may happen.    
    public interface IPostgresqlWrapper
    {
        OutputResponse AddRequest(AddDocumentInbound addDocumentInbound);
        OutputResponse UpdateResponse(AddDocumentOutbound addDocumentOutbound);       
    }
}
