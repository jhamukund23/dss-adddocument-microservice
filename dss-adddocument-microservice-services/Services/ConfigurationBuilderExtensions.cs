using dss_adddocument_microservice_services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

namespace dss_adddocument_microservice.services
{
    // Extends IConfigurationBuilder to add Vault support.   
    [ExcludeFromCodeCoverage]
    public static class ConfigurationBuilderExtensions
    {
        public readonly static string VaultClientToken = Environment.GetEnvironmentVariable("VAULT_CLIENT_TOKEN");
        public readonly static string VaultHttpPostPath = Environment.GetEnvironmentVariable("VAULT_LOGIN_URL");
        public readonly static string VaultEnvironment = Environment.GetEnvironmentVariable("VAULT_ENVIRONMENT");
        public readonly static string VaultUrl = Environment.GetEnvironmentVariable("VAULT_URL");
        public readonly static string VaultPath = Environment.GetEnvironmentVariable("VAULT_PATH");
        public readonly static string VaultRole = Environment.GetEnvironmentVariable("VAULT_ROLE");
        public static IConfigurationBuilder AddHashiVault(this IConfigurationBuilder configurationBuilder, bool asJson = false, EngineVersions version = EngineVersions.V2)
        {
            var token = GetToken();
            var paths = VaultPath?.Split(';');
            return configurationBuilder.AddVaultWithToken(VaultUrl, token, paths, VaultEnvironment, version, asJson);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configurationBuilder"></param>
        /// <param name="vaultClient"></param>
        /// <param name="manager"></param>
        /// <param name="paths"></param>
        /// <param name="asJson"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddHashiVault(
            this IConfigurationBuilder configurationBuilder,
            IVaultClient vaultClient,
            IVaultManager manager,
            string[] paths,
            string sNamespace,
            EngineVersions version,
            bool asJson = false)
        {
            configurationBuilder.Add(new VaultConfigurationSource()
            {
                Client = vaultClient,
                Manager = manager,
                Paths = paths,
                Namespace = sNamespace,
                AsJson = asJson,
                Version = version
            });

            return configurationBuilder;
        }

        private static string GetToken()
        {
            dynamic result = null;
            Dictionary<string, string> postData = new()
            {
                    { "role", VaultRole },
                    { "jwt", VaultClientToken }
                };
            //Base url will change based on Vault environment you are calling
            HttpResponseMessage responseMessage = HttpPost(VaultHttpPostPath, postData, VaultEnvironment);

            if (responseMessage.StatusCode == HttpStatusCode.OK)
            {
                result = JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            }
            return result.auth.client_token.ToString();
        }

        // Overridng the Http POST 
        public static HttpResponseMessage HttpPost(string url, dynamic postData, string vaultNamespace = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpClient httpClient = new();
            if (vaultNamespace != null)
            { httpClient.DefaultRequestHeaders.Add("X-Vault-Namespace", vaultNamespace); }
            var postContent = JsonConvert.SerializeObject(postData);
            HttpResponseMessage responseMessage = httpClient.PutAsync(url, new StringContent(postContent, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
            return responseMessage;
        }
        /// <summary>
        /// Uses token authentication for Vault. This authentication scheme has been deprecated by Humana
        /// and we should start using mTLS authentication instead.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="vaultUri"></param>
        /// <param name="token"></param>
        /// <param name="paths"></param>
        /// <param name="asJson"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddVaultWithToken(
            this IConfigurationBuilder builder,
            string vaultUri,
            string token,
            string[] paths,
            string sNamespace,
            EngineVersions version,
            bool asJson = false)
        {
            var authInfo = new TokenAuthMethodInfo(token);
            var vaultClientSettings = new VaultClientSettings(vaultUri, authInfo);
            var vaultClient = new VaultClient(vaultClientSettings);
            return AddHashiVault(builder, vaultClient, new VaultManager(), paths, sNamespace, version, asJson);
        }
    }
}
