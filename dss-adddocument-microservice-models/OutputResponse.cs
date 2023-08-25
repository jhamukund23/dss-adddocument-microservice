using System.Diagnostics.CodeAnalysis;

namespace dss_adddocument_microservice.models
{
    // This class contains stored procedure response.
    [ExcludeFromCodeCoverage]
    public class OutputResponse
    {
        public Int64 DocumentId { get; set; }
        public short DocVersion { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
        public DateTime? DateAdded { get; set; }
    }
}
