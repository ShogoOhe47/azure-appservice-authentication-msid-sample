using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
     * access token:
     * first time: https://learn.microsoft.com/ja-jp/entra/identity-platform/v2-oauth2-client-creds-grant-flow#first-case-access-token-request-with-a-shared-secret
     * refresh: https://learn.microsoft.com/ja-jp/entra/identity-platform/v2-oauth2-client-creds-grant-flow#use-a-token
     * 
     * access from client:
     * https://learn.microsoft.com/azure/app-service/configure-authentication-customize-sign-in-out#client-directed-sign-in
     */
    class ExampleHttpClient : ICallAuthProviderSample
    {
        private static String Instance = "";
        private static String AppServiceClientId = "";
        private static readonly HttpClient httpClient = new HttpClient();

        public ExampleHttpClient(String AADCloudInstance, String AppServiceAuthenticationClientId)
        {
            Instance = AADCloudInstance;
            AppServiceClientId = AppServiceAuthenticationClientId;
        }


        public async Task<String> getToken(string TenantId, string ClientId, string Secret)
        {
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                // build uri: /{tenant}/oauth2/v2.0/token
                Uri OAuthUri = new Uri(Instance + "" + TenantId + "/oauth2/v2.0/token");

                // build POST contents
                String contentString = $"client_id={ClientId}&scope={AppServiceClientId}%2F.default" // https%3A%2F%2Fgraph.microsoft.com%2F.default
                    + $"&client_secret={Secret}&grant_type=client_credentials";
                StringContent content = new StringContent(contentString, Encoding.UTF8, @"application/x-www-form-urlencoded");

                // HttpResponseMessage response = await httpClient.PostAsync(OAuthUri, content);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, OAuthUri);
                request.Content = content;
                HttpResponseMessage response = await httpClient.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine("authentication is failed");
                }
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                Console.WriteLine(responseBody);

                Dictionary<string, JsonElement> jwtDictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseBody);

                String accessToken = jwtDictionary["access_token"].ToString();

                return accessToken;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }

            return "";
        }

        Task<string> ICallAuthProviderSample.accessAppService(Uri AppServiceUri, string accessToken)
        {
            return ExampleHttpClientSharedCode.accessAppService(httpClient, AppServiceUri, accessToken);
        }
    }
}