using System.Diagnostics.CodeAnalysis;

namespace dss_adddocument_microservice.models
{
    // AppSettings contains all configuration released properties.
    [ExcludeFromCodeCoverage]
    public class AppSetting
    {
        // DB
        public string DB_SERVER_NAME { get; set; }
        public string DB_DATABASE { get; set; }
        public string DB_SERVER_PORT { get; set; }
        public string DB_USER_ID { get; set; }
        public string DB_PASSWORD { get; set; }       

        // KAFKA
        public string KAFKA_TOPIC_NAME { get; set; }
        public string KAFKA_BOOTSTRAPSERVER { get; set; }
        public string KAFKA_SASL_USER_NAME { get; set; }
        public string KAFKA_SASL_PASSWORD { get; set; }
        public string KAFKA_GROUP_ID { get; set; }

        public string KAFKA_TOPIC_INBOUND { get; set; }
        public string KAFKA_TOPIC_OUTBOUND { get; set; }


        // Azure Authentication 
        public string AZURE_CLIENT_ID { get; set; }
        public string AZURE_TENANT_ID { get; set; }
        public string AZURE_CLIENT_SECRET { get; set; }

        //Azure Storage
        public string STORAGE_ACCOUNTNAME { get; set; }
        public string BLOB_CONTAINERNAME { get; set; }


    }
}

