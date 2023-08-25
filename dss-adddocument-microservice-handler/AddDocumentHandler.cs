using dss_adddocument_microservice.data;
using dss_adddocument_microservice.models;
using dss_adddocument_microservice.services;
using dss_adddocument_microservice_models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
namespace dss_adddocument_microservice.handler
{
    public class AddDocumentHandler : IKafkaHandler<string, AddDocumentInbound>
    {
        private readonly IKafkaProducer<string, AddDocumentOutbound> _responseProducer;
        private readonly IAzureStorage _azureStorage;
        private readonly IPostgresqlWrapper _postgresqlWrapper;
        private readonly IOptions<AppSetting> _appSetting;
        private readonly ILogger<AddDocumentHandler> _logger;
        private readonly IValidator<AddDocumentInbound> _validator;
        private const string errorLog = "AddDocumentHandler -> {methodName} -> ExceptionMessage: {exceptionMessage}";

        public AddDocumentHandler(
             IKafkaProducer<string, AddDocumentOutbound> responseProducer,
             IAzureStorage azureStorage,
             IPostgresqlWrapper iPostgresqlWrapper,
             IOptions<AppSetting> appSetting,
             ILogger<AddDocumentHandler> logger,
             IValidator<AddDocumentInbound> validator
            )
        {
            _azureStorage = azureStorage;
            _responseProducer = responseProducer;
            _postgresqlWrapper = iPostgresqlWrapper;
            _appSetting = appSetting;
            _logger = logger;
            _validator = validator;
        }
        public Task HandleAsync(string key, AddDocumentInbound value)
        {
            OutputResponse outputResponse = new();
            try
            {
                ValidationResult result = _validator.Validate(value);
                if (!result.IsValid)
                {
                    StringBuilder sb = new();
                    result.Errors.ToList().ForEach(x => sb.AppendLine(x.ErrorMessage));                   
                    throw new DssRequestValidationException(sb.ToString().TrimEnd(',').Replace('\n', ' ').Replace('\r', ' '));
                }
                else
                {
                    
                    // Call AddRequest method to insert request details into database table.                    
                    outputResponse = _postgresqlWrapper.AddRequest(value);

                    if (outputResponse != null && !string.IsNullOrEmpty(outputResponse.ErrorMsg) && outputResponse.ErrorCode != 0)
                    {
                        throw new DssDBException("adddocument storedprocedure return error code.");
                    }

                    // Rearrange all the different directory names and put them all together and assign them to the new variable.
                    var dirStructure = value.Source 
                                        + "/" + Convert.ToDateTime(outputResponse.DateAdded).ToString("yyyyMMdd") 
                                        + "/" + outputResponse.DocumentId 
                                        + "/" + outputResponse.DocVersion;

                    // Generate and return the SAS URI.                   
                    var sasUri = _azureStorage
                        .GetUserDelegationSasDirectory(
                        _appSetting.Value.STORAGE_ACCOUNTNAME,
                        _appSetting.Value.BLOB_CONTAINERNAME,
                        dirStructure
                        );

                    // Prepare response for outbound kafka topic.  
                    // Send response to Kafka Outbound topic.
                    // Update response into DSSResponse database table.
                    ProduceAndUpdateResponse(ResponseBuilder(value, true, outputResponse, sasUri));
                }
            }
            catch (DssRequestValidationException ex)
            {
                _logger.LogError(ex, errorLog, "HandleAsync -> Validate", ex.Message);
                outputResponse.ErrorCode = Errors.Request_Invalid_Error_Code;
                outputResponse.ErrorMsg = ex.Message;
                ProduceAndUpdateResponse(ResponseBuilder(value, false, outputResponse, null));
            }
            catch (DssDBException ex)
            {
                _logger.LogError(ex, errorLog, "HandleAsync -> AddRequest", ex.Message);
                outputResponse.ErrorMsg = Errors.GetErrorDescriptionById(outputResponse.ErrorCode);
                ProduceAndUpdateResponse(ResponseBuilder(value, false, outputResponse, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, errorLog, "HandleAsync", ex.Message);
                outputResponse.ErrorCode = Errors.Unhandled_Error_Code;
                outputResponse.ErrorMsg = Errors.GetErrorDescriptionById(outputResponse.ErrorCode);
                ProduceAndUpdateResponse(ResponseBuilder(value, false, outputResponse, null));
            }
            return Task.CompletedTask;
        }

        #region Private Method       

        [ExcludeFromCodeCoverage]
        private AddDocumentOutbound ResponseBuilder(AddDocumentInbound value, bool isSuccess, OutputResponse outputResponse, Uri sasUri)
        {
            AddDocumentOutbound addDocumentOutbound = new();
            try
            {
                if (value != null)
                {
                    addDocumentOutbound.SpecVersion = value.SpecVersion;
                    addDocumentOutbound.Type = value.Type;
                    addDocumentOutbound.Source = value.Source;
                    addDocumentOutbound.Id = Guid.NewGuid().ToString();
                    addDocumentOutbound.Time = value.Time;
                    addDocumentOutbound.Subject = value.Subject;
                    addDocumentOutbound.DataSchema = value.DataSchema;
                    addDocumentOutbound.DataContentType = value.DataContentType;
                    addDocumentOutbound.Response.RequestId = value.Id;
                    addDocumentOutbound.Response.Success = isSuccess;
                }
                if (sasUri != null && outputResponse != null)
                {
                    addDocumentOutbound.Response.DocId = outputResponse.DocumentId;
                    addDocumentOutbound.Response.Version = outputResponse.DocVersion;
                    addDocumentOutbound.Response.Sas = sasUri.ToString();
                    addDocumentOutbound.Response.Error = null;
                }
                if (!isSuccess)
                {                  
                    addDocumentOutbound.Response.Sas = null;
                    addDocumentOutbound.Response.Error.ErrorCode = outputResponse.ErrorCode;
                    addDocumentOutbound.Response.Error.ErrorDescription = outputResponse.ErrorMsg;
                }
                return addDocumentOutbound;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, errorLog, "PrepareOutboundMessages", ex.Message);
                throw;
            }
        }

        // Send response message to Kafka response topic.
        // Update response in database table.
        [ExcludeFromCodeCoverage]
        private void ProduceAndUpdateResponse(AddDocumentOutbound responseModel)
        {
            try
            {
                if (responseModel != null)
                {
                    // Send response to Kafka Outbound topic.
                    _responseProducer.ProduceAsync(_appSetting.Value.KAFKA_TOPIC_OUTBOUND, responseModel.Id, responseModel);

                    // Call UpdateResponse method to update response into DSSResponse database table. 
                    var outputResponse = _postgresqlWrapper.UpdateResponse(responseModel);
                    if (!string.IsNullOrEmpty(outputResponse.ErrorMsg) && outputResponse.ErrorCode != 0)
                    {
                        throw new DssDBException("update_response storedprocedure return error.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, errorLog, "ProduceAndUpdateResponse", ex.Message);
            }
        }
       
        #endregion
     
    }

}


