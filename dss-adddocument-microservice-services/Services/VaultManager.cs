using dss_adddocument_microservice.services;
using System.Diagnostics.CodeAnalysis;

namespace dss_adddocument_microservice.services
{
    // This class for load secret.
    [ExcludeFromCodeCoverage]
    public class VaultManager : IVaultManager
    {
        public bool Load(IDictionary<string, object> value, string key) => true;
    }
}
