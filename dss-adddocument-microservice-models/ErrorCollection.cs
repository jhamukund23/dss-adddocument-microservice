using System.Diagnostics.CodeAnalysis;

namespace dss_adddocument_microservice_models
{
    // ErrorCollection class have list of error code with error description
    [ExcludeFromCodeCoverage]
    public class ErrorCollection
    {
        public int ErrorCode { get; set; }
        public string Description { get; set; }
    }
    // Error class have list of error code with error description
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static int Request_Invalid_Error_Code { get; set; } = 412;
        public static int Unhandled_Error_Code { get; set; } = 500;
        private static readonly List<ErrorCollection> errors = new()
         {
         new ErrorCollection() { ErrorCode = 1001, Description = "Please check id and type are valid." },
         new ErrorCollection() { ErrorCode = 1002, Description = "Error occurred while processing the request Json." },
         new ErrorCollection() { ErrorCode = 1003, Description = "Consumer is not authorized to perform this operation." },
         new ErrorCollection() { ErrorCode = 1004, Description = "There was an error assigning DocId." },
         new ErrorCollection() { ErrorCode = 1005, Description = "Error occurred while processing the request." },
         new ErrorCollection() { ErrorCode = 412,  Description = "Please check the request for valid values." },
         new ErrorCollection() { ErrorCode = 500 , Description = "There was an error while processing the request. Please contact support with request id." }
        };

        // Return error details by error code.
        public static string GetErrorDescriptionById(int ErrorCode)
        {
            return errors.FirstOrDefault(x => x.ErrorCode==ErrorCode).Description;
        }
    }
}
