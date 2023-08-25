namespace dss_adddocument_microservice.services
{
    /// <summary>
    /// An interface representing common blob storage actions that may happen.
    /// </summary>
    public interface IAzureStorage
    {
        Uri GetSasUriForDirectory(string connectionString, string fileSystemName, string directoryPath);
        void CreateSubDirectories(string storageAccountName, string fileSystemName, string directoryPath);
        Uri GetUserDelegationSasDirectory(string storageAccountName,string fileSystemName, string directoryPath);

    }
}

