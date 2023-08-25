using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace dss_adddocument_microservice.models
{
    // AddDocumentInbound for the kafka error topic.
    [ExcludeFromCodeCoverage]
    public class AddDocumentInbound
    {
        //To access child class property create child class object inside parent class constructor.
        public AddDocumentInbound()
        {
            Request = new Request();
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
        public Request Request { get; set; }
    }
    // Request contains properties that contain the request details.
    [ExcludeFromCodeCoverage]
    public class Request
    {
        [JsonProperty("filename")]
        public string FileName { get; set; }
        [JsonProperty("docid")]
        public string DocId { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("filesize")]
        public string FileSize { get; set; }
        [JsonProperty("userid")]
        public string UserId { get; set; }
        [JsonProperty("syscode")]
        public string SysCode { get; set; }
        [JsonProperty("spn")]
        public string Spn { get; set; }
    }
}

