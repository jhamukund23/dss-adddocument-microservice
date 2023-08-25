using dss_adddocument_microservice.services;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;
using VaultSharp;

namespace dss_adddocument_microservice_services.Services
{
    /// Configuration used by the provider to get secrets within Vault service.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class VaultConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// Vault client
        /// </summary>
        public IVaultClient Client { get; set; }

        /// <summary>
        /// Vault secret manager impl
        /// </summary>
        public IVaultManager Manager { get; set; }

        /// <summary>
        /// Secret paths
        /// </summary>
        public string[] Paths { get; set; }

        /// <summary>
        /// Whether or not to load secrets in JSON format.
        /// </summary>
        public bool AsJson { get; set; }

        /// <summary>
        /// Version of KV we should use.
        /// </summary>
        public EngineVersions Version { get; set; }

        public string Namespace { get; set; }

        /// <summary>
        /// Returns a provider to load secrets from Vault service.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new VaultConfigurationProvider(Client, Manager, Paths, Namespace, AsJson, Version);
        }
    }
}
