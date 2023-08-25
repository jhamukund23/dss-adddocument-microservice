using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

[assembly: CLSCompliant(true)]
namespace dss_adddocument_microservice.models
{
    // AddDocumentOutbound for the kafka response topic. 
    [ExcludeFromCodeCoverage]
    public class AddDocumentOutbound
    {
        //To access child class property create child class object inside parent class constructor.
        public AddDocumentOutbound()
        {
            Response = new Response();
        }
        [JsonProperty("specversion")]
        public string SpecVersion { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("source")]
        public string Source { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("time")]
        public DateTime Time { get; set; }
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("dataschema")]
        public string DataSchema { get; set; }
        [JsonProperty("datacontenttype")]
        public string DataContentType { get; set; }

        [JsonProperty("data")]
        public Response Response { get; set; }
    }

    // Response contains properties that contain the actual response.
    [ExcludeFromCodeCoverage]
    public class Response
    {
        public Response()
        {
            Error = new Error();
        }

        [JsonProperty("requestid")]
        public string RequestId { get; set; }

        [JsonProperty("docid")]
        public Int64 DocId { get; set; }

        [JsonProperty("version")]
        public short Version { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("sas")]
        public string Sas { get; set; }
        
        [JsonProperty("error")]
        public Error Error { get; set; }
    }
    // Error contains properties that contain the actual error.
    [ExcludeFromCodeCoverage]
    public class Error
    {
        [JsonProperty("errorcode")]
        public int ErrorCode { get; set; }
        [JsonProperty("errordescription")]
        public string ErrorDescription { get; set; }
    }
}
