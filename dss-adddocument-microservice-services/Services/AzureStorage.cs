using Azure.Core;
using Azure.Identity;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Azure.Storage.Sas;
using dss_adddocument_microservice.models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace dss_adddocument_microservice.services
{
    [ExcludeFromCodeCoverage]
    // Utility for Blob Processing.    
    public class AzureStorage : IAzureStorage
    {
        #region CTOR
        private readonly ILogger<AzureStorage> _logger;
        private readonly IOptions<AppSetting> _appSetting;
        public AzureStorage(
            ILogger<AzureStorage> logger,
            IOptions<AppSetting> appSetting
            )
        {
            _logger = logger;
            _appSetting = appSetting;
        }

        #endregion

        #region Generate SAS Uri For ADLS Gen2

        //Generate a user delegation SAS for a directory when a hierarchical namespace is enabled for the storage account.
        public Uri GetUserDelegationSasDirectory(string storageAccountName, string fileSystemName, string directoryPath)
        {
            try
            {
                TokenCredential credential = new ClientSecretCredential(
                       _appSetting.Value.AZURE_TENANT_ID,
                       _appSetting.Value.AZURE_CLIENT_ID,
                       _appSetting.Value.AZURE_CLIENT_SECRET,
                       new TokenCredentialOptions());

                string directoryUri = "https://" +storageAccountName+ ".blob.core.windows.net/" + fileSystemName + "//" + directoryPath;
                var directoryClient = new DataLakeDirectoryClient(new Uri(directoryUri), credential);

                // Get service endpoint from the directory URI.
                DataLakeUriBuilder dataLakeServiceUri = new(directoryClient.Uri)
                {
                    FileSystemName = null,
                    DirectoryOrFilePath = null
                };
                // Get service client.
                DataLakeServiceClient dataLakeServiceClient = new(dataLakeServiceUri.ToUri(), credential);

                // Get a user delegation key that's valid for 2 hours.
                // You can use the key to generate any number of shared access signatures 
                // over the lifetime of the key.
                UserDelegationKey userDelegationKey = dataLakeServiceClient.GetUserDelegationKey(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2));

                // Create a SAS token that's valid for 2 hours.
                DataLakeSasBuilder sasBuilder = new()
                {
                    // Specify the file system name and path, and indicate that
                    // the client object points to a directory.
                    FileSystemName = directoryClient.FileSystemName,
                    Resource = "d",
                    IsDirectory = true,
                    Path = directoryClient.Path,
                    Protocol= SasProtocol.Https,
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(2)
                };

                // Specify rwl permissions for the SAS.
                sasBuilder.SetPermissions(
                    DataLakeSasPermissions.Read |
                    DataLakeSasPermissions.Write |
                    DataLakeSasPermissions.List
                    );

                // Construct the full URI, including the SAS token.
                DataLakeUriBuilder fullUri = new(directoryClient.Uri)
                {
                    Sas = sasBuilder.ToSasQueryParameters(userDelegationKey, dataLakeServiceClient.AccountName)
                };
                return fullUri.ToUri();
            }
            catch (Exception ex)
            {
                _logger.LogError("AzureStorage -> GetUserDelegationSasDirectory Error : " + ex.Message);
                throw;

            }
        }

        //Create a service SAS for a directory.
        public Uri GetSasUriForDirectory(string connectionString, string fileSystemName, string directoryPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(fileSystemName) && !string.IsNullOrEmpty(directoryPath))
                {
                    //Creates a client to the DataLakeDirectory using the connection string, fileSystemName and path.
                    //DataLakeDirectoryClient must be authorized with Shared Key credentials to create a service SAS.
                    DataLakeDirectoryClient directoryClient = new(connectionString, fileSystemName, directoryPath);

                    if (directoryClient.CanGenerateSasUri)
                    {
                        //Defines the resource being accessed and for how long the access is allowed.
                        DataLakeSasBuilder sasBuilder = new()
                        {
                            // Specify the file system name, the path, and indicate that
                            // the client object points to a directory.
                            FileSystemName = directoryClient.FileSystemName,
                            Resource = "d",
                            IsDirectory = true,
                            Path = directoryClient.Path,
                            Protocol= SasProtocol.Https,
                            ExpiresOn = DateTimeOffset.UtcNow.AddHours(2)
                        };

                        // If no stored access policy is specified, create the policy
                        // by specifying expiry and permissions.
                        sasBuilder.SetPermissions(DataLakeSasPermissions.Read |
                                                  DataLakeSasPermissions.Write |
                                                  DataLakeSasPermissions.List);

                        // Get the SAS URI for the specified directory.
                        Uri sasUri = directoryClient.GenerateSasUri(sasBuilder);
                        return sasUri;
                    }
                    else
                    {
                        throw new ArgumentException("client not authorized with Shared Key credentials to create a service SAS");
                    }
                }
                else
                {
                    throw new ArgumentException("bad argument");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("AzureStorage -> GetSasUriForDirectory Error : " + ex.Message);
                throw;
            }
        }

        #endregion

        #region Create Sub Directories ADLS Gen2 Storage
        //Connectionstring  : Datalake connection string
        //fileSystemName : "ContainerName"
        //directoryName : "drname1/drname2"
        public void CreateSubDirectories(string storageAccountName, string fileSystemName, string directoryPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(fileSystemName) && !string.IsNullOrEmpty(directoryPath))
                {
                    // Create a DataLakeServiceClient object.
                    TokenCredential credential = new ClientSecretCredential(
                        _appSetting.Value.AZURE_TENANT_ID,
                        _appSetting.Value.AZURE_CLIENT_ID,
                        _appSetting.Value.AZURE_CLIENT_SECRET,
                        new TokenCredentialOptions());

                    string dfsUri = "https://" + storageAccountName + ".blob.core.windows.net";
                    var storageAccount = new DataLakeServiceClient(new Uri(dfsUri), credential);

                    string[] strPath = fileSystemName.Split("/");
                    // Create a DataLakeDirectoryClient object for the container that you want to create the subdirectory in.
                    DataLakeFileSystemClient dirClient = storageAccount.GetFileSystemClient(strPath[0]);
                    if (strPath.Length == 1)
                    {
                        DataLakeDirectoryClient directory = dirClient.GetDirectoryClient(directoryPath);
                        directory.CreateIfNotExists();
                    }
                    else
                    {
                        DataLakeDirectoryClient directory = dirClient.GetDirectoryClient(strPath[1]);
                        // Call the CreateSubDirectory() method on the DataLakeDirectoryClient object.
                        directory.CreateSubDirectory(directoryPath);
                    }
                }
                else
                {
                    throw new ArgumentException("bad argument");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, message: ex.Message);
                throw;
            }
        }

        #endregion
    }
}
