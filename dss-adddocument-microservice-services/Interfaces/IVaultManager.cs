namespace dss_adddocument_microservice.services
{
    public interface IVaultManager
    {
        bool Load(IDictionary<string, object> value, string key);
    }

    public enum EngineVersions
    {
        V1,
        V2
    }
}
