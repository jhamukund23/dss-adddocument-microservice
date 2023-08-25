using Azure.Core;
using Azure.Identity;
using dss_adddocument_microservice.models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace dss_adddocument_microservice.data
{
    // This class for return connection string. 
    [ExcludeFromCodeCoverage]
    public class DBHelper : IDBHelper
    {
        private DateTime dtExpiresOn;
        private readonly IOptions<AppSetting> _appSetting;
        private readonly ILogger<DBHelper> _logger;
        public string DBConnectionString { get; set; }
        public string PostgreSqlDBConnectionString { get; set; }
        public DBHelper()
        {
            DBConnectionString = String.Empty;
            PostgreSqlDBConnectionString = String.Empty;
        }
        public DBHelper(IOptions<AppSetting> appSettingData, ILogger<DBHelper> logger)
        {
            this._appSetting = appSettingData;
            this._logger = logger;
        }
        // This method return postgresql db connection string.
        public string GetConnectionString()
        {
            if (IsExpiry())
            {
                if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development")
                {
                    DBConnectionString = "Server={0};Database={1};Port={2};User Id={3};Password=;Ssl Mode=Require;Trust Server Certificate=true;";
                    PostgreSqlDBConnectionString = string.Format(DBConnectionString, _appSetting.Value.DB_SERVER_NAME, _appSetting.Value.DB_DATABASE, _appSetting.Value.DB_SERVER_PORT, _appSetting.Value.DB_USER_ID);
                }
                else
                {
                    string db_accesstoken = GetToken();
                    DBConnectionString = "Server={0};Database={1};Port={2};User Id={3};Password={4};Ssl Mode=Require;Trust Server Certificate=true;";
                    PostgreSqlDBConnectionString = string.Format(DBConnectionString, _appSetting.Value.DB_SERVER_NAME, _appSetting.Value.DB_DATABASE, _appSetting.Value.DB_SERVER_PORT, _appSetting.Value.DB_USER_ID, db_accesstoken);
                }
                return PostgreSqlDBConnectionString;
            }
            else
            {
                return PostgreSqlDBConnectionString;
            }
        }
        // Below method to return token.
        public string GetToken()
        {
            DateTimeOffset dt;
            try
            {
                var credential = new ClientSecretCredential(_appSetting.Value.AZURE_TENANT_ID, _appSetting.Value.AZURE_CLIENT_ID, _appSetting.Value.AZURE_CLIENT_SECRET);
                var db_accesstoken = (credential.GetToken(
                   new TokenRequestContext(scopes: new string[] { "https://ossrdbms-aad.database.windows.net/.default" }) { })).Token;
                dt = (credential.GetToken(
               new TokenRequestContext(scopes: new string[] { "https://ossrdbms-aad.database.windows.net/.default" }) { })).ExpiresOn;
                dtExpiresOn = dt.UtcDateTime;
                return db_accesstoken;

            }
            catch (Exception ex)
            {
                _logger.LogError("DBHepler Error : " + ex.Message);
                throw;
            }
        }
        // Below method to check token expiry.
        public bool IsExpiry()
        {
            return dtExpiresOn < DateTime.UtcNow;
        }
    }
}
