using Confluent.Kafka;
using dss_adddocument_microservice.data;
using dss_adddocument_microservice.handler;
using dss_adddocument_microservice.models;
using dss_adddocument_microservice.services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
//setting CLSCompliant attribute to true
[assembly: CLSCompliant(true)]
namespace dss_adddocument_microservice
{
    // Program class is entry point in this application.
    [ExcludeFromCodeCoverage]
    public class Program
    {
        protected Program() { }

        // Main method call when program.cs file execute.
        public static void Main(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            // Access and read appsettings.json file using ConfigurationBuilder 
            var configuration = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddHashiVault()
                  .AddJsonFile($"appsettings.json", true, true)
                  .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                  .AddEnvironmentVariables()
                  .Build();

            // This method gets called by the runtime. Use this method to add services to the container.
            Host.CreateDefaultBuilder(args)
           .ConfigureServices((_, services) =>
           {
               // Reading the vault and assigning it to the Appsetting 
               services.Configure<AppSetting>(options =>
               {
                   options.DB_SERVER_NAME = configuration["DB_SERVER_NAME"]??Environment.GetEnvironmentVariable("DB_SERVER_NAME");
                   options.DB_DATABASE = configuration["DB_DATABASE"]??Environment.GetEnvironmentVariable("DB_DATABASE");
                   options.DB_SERVER_PORT = configuration["DB_SERVER_PORT"]??Environment.GetEnvironmentVariable("DB_SERVER_PORT");
                   options.DB_USER_ID = configuration["DB_USER_ID"]??Environment.GetEnvironmentVariable("DB_USER_ID");
                   options.DB_PASSWORD = configuration["DB_PASSWORD"];

                   options.KAFKA_BOOTSTRAPSERVER = configuration["KAFKA_BOOTSTRAPSERVER"]??Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAPSERVER");
                   options.KAFKA_SASL_PASSWORD = configuration["KAFKA_SASL_PASSWORD"];
                   options.KAFKA_SASL_USER_NAME = configuration["KAFKA_SASL_USER_NAME"]??Environment.GetEnvironmentVariable("KAFKA_SASL_USER_NAME");
                   options.KAFKA_GROUP_ID = configuration["KAFKA_GROUP_ID"]??Environment.GetEnvironmentVariable("KAFKA_GROUP_ID");
                   options.KAFKA_TOPIC_INBOUND = configuration["KAFKA_TOPIC_INBOUND"]??Environment.GetEnvironmentVariable("KAFKA_TOPIC_INBOUND");
                   options.KAFKA_TOPIC_OUTBOUND = configuration["KAFKA_TOPIC_OUTBOUND"]??Environment.GetEnvironmentVariable("KAFKA_TOPIC_OUTBOUND");

                   options.AZURE_CLIENT_ID = configuration["AZURE_CLIENT_ID"]??Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
                   options.AZURE_TENANT_ID = configuration["AZURE_TENANT_ID"]??Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
                   options.AZURE_CLIENT_SECRET = configuration["AZURE_CLIENT_SECRET"];

                   options.BLOB_CONTAINERNAME = configuration["BLOB_CONTAINERNAME"]??Environment.GetEnvironmentVariable("BLOB_CONTAINERNAME");
                   options.STORAGE_ACCOUNTNAME = configuration["STORAGE_ACCOUNTNAME"]??Environment.GetEnvironmentVariable("STORAGE_ACCOUNTNAME");
               });


               // Read Kafka configuration details form configuration file and bind in clientConfig class properties.
               var clientConfig = new ClientConfig
               {
                   BootstrapServers = configuration["KAFKA_BOOTSTRAPSERVER"]??Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAPSERVER"),
                   SaslUsername = configuration["KAFKA_SASL_USER_NAME"]??Environment.GetEnvironmentVariable("KAFKA_SASL_USER_NAME"),
                   SaslPassword = configuration["KAFKA_SASL_PASSWORD"],
                   SecurityProtocol = SecurityProtocol.SaslSsl,
                   SaslMechanism = SaslMechanism.Plain,                  
                   SslCaLocation= Directory.GetCurrentDirectory() + "/DigiCert Global Root G2.cer"
               };

               // Read Kafka configuration details form configuration file and bind in consumerConfig class properties.
               var consumerConfig = new ConsumerConfig(clientConfig)
               {
                   GroupId = configuration["KAFKA_GROUP_ID"]??Environment.GetEnvironmentVariable("KAFKA_GROUP_ID"),
                   EnableAutoCommit = true,
                   AutoOffsetReset = AutoOffsetReset.Earliest
               };

               var producerConfig = new ProducerConfig(clientConfig)
               {
                   // Best practice for Kafka producer to prevent data loss.                   
                   Acks = Acks.All
               };

               // Add services to the container.
               services.AddSingleton(producerConfig);
               services.AddSingleton(typeof(IKafkaProducer<,>), typeof(KafkaProducer<,>));

               services.AddSingleton(consumerConfig);
               services.AddSingleton(typeof(IKafkaConsumer<,>), typeof(KafkaConsumer<,>));

               services.AddScoped<IKafkaHandler<string, AddDocumentInbound>, AddDocumentHandler>();
               services.AddHostedService<AddDocumentConsumer>();

               services.AddTransient<IValidator<AddDocumentInbound>, AddDocumentInboundValidator>();
               services.AddTransient<IAzureStorage, AzureStorage>();
               services.AddTransient<IPostgresqlWrapper, PostgresqlWrapper>();
               services.AddSingleton<IDBHelper, DBHelper>();
           })
            .Build()
            .Run();
        }
    }
}
