namespace dss_adddocument_microservice.data
{
    /// Provides mechanism to get postgresql connectionstring.
    public interface IDBHelper
    {
        string GetConnectionString();
        string GetToken();
    }
}
