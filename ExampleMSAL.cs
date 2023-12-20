using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace azure_appservice_authentication_msid_sample
{
    /* Code sample for App Service Authentication (Easy Auth) with Microsoft Entra ID platform.
     * https://learn.microsoft.com/azure/app-service/overview-authentication-authorization
     * 
     * https://learn.microsoft.com/entra/msal/dotnet/getting-started/initializing-client-applications
     */
    class ExampleMSAL : ICallAuthProviderSample
    {
        private static String Instance = "";
        private static String AppServiceClientId = "";
        private static readonly HttpClient httpClient = new HttpClient();

        public ExampleMSAL(String AADCloudInstance, String AppServiceAuthenticationClientId)
        {
            Instance = AADCloudInstance;
            AppServiceClientId = AppServiceAuthenticationClientId;
        }

        public async Task<string> getToken(string TenantId, string ClientId, string Secret)
        {
            // https://learn.microsoft.com/en-us/entra/msal/dotnet/getting-started/initializing-client-applications
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(ClientId)
                .WithClientSecret(Secret)
                .Build();

            // https://learn.microsoft.com/en-us/entra/msal/dotnet/acquiring-tokens/web-apps-apis/client-credential-flows
            AuthenticationResult authResult = await app.AcquireTokenForClient(scopes: new[] { $"{AppServiceClientId}/.default" })        // uses the token cache automatically, which is optimized for multi-tenant access
                   .WithAuthority(AzureCloudInstance.AzurePublic, TenantId)  // do not use "common" or "organizations"!
                   .ExecuteAsync();

            return authResult.AccessToken;
        }

        public Task<String> accessAppService(Uri AppServiceUri, String accessToken)
        {
            return ExampleHttpClientSharedCode.accessAppService(httpClient, AppServiceUri, accessToken);
        }
    }
}