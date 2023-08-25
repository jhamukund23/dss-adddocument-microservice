using Microsoft.Extensions.Configuration;
using System.Text.Json;
using VaultSharp;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace dss_adddocument_microservice.services
{
    // Loads secrets from Vault service
    [ExcludeFromCodeCoverage]
    internal class VaultConfigurationProvider : ConfigurationProvider
    {
        private readonly IVaultClient _client;
        private readonly IVaultManager _manager;
        private readonly string[] _paths;
        private readonly bool _asJson;
        private readonly string _namespace;
        private readonly EngineVersions _version;

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="client"></param>
        /// <param name="manager"></param>
        /// <param name="paths"></param>
        /// <param name="asJson"></param>
        /// <param name="version"></param>
        public VaultConfigurationProvider(

            IVaultClient client,
            IVaultManager manager,
            string[] paths,
            string sNamespace,
            bool asJson = false,
            EngineVersions version = EngineVersions.V2)
        {
            _client = client;
            _manager = manager;
            _paths = paths;
            _asJson = asJson;
            _namespace = sNamespace;
            _version = version;
            _client.Settings.Namespace = _namespace;
        }

        /// <summary>
        /// Overridden to load secrets from Vault service.
        /// </summary>
        public override void Load() => LoadTokensAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Loads secrets from Vault service for the Paths configured in CTOR.
        /// </summary>
        /// <returns></returns>
        async Task LoadTokensAsync()
        {
            IDictionary<string, string> data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var path in _paths)
            {
                IDictionary<string, object> secretsDictionary;
                if (_version == EngineVersions.V1)
                {
                    var secrets = await _client.V1.Secrets.KeyValue.V1.ReadSecretAsync(path, "secret").ConfigureAwait(false);
                    secretsDictionary = secrets.Data;
                }
                else
                {
                    var secrets = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(path, mountPoint: "secret").ConfigureAwait(false);
                    secretsDictionary = secrets.Data.Data;
                }

                MergeNewTokensWithData(data, secretsDictionary);
            }
            Data = data;
        }

        private void MergeNewTokensWithData(IDictionary<string, string> data, IDictionary<string, object> secrets)
        {
            foreach (var secret in secrets.Where(secret => _manager.Load(secrets, secret.Key)))
            {
                var textualSecret = secret.Value.ToString();
                if (_asJson)
                {
                    JsonSerializer.Deserialize<Dictionary<string, string>>(textualSecret);
                }
                else
                {
                    data.Add(secret.Key, textualSecret);
                }
            }
        }
    }
}
