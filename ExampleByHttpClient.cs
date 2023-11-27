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
    internal class ExampleByHttpClient
    {
        static readonly HttpClient httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            // read appsettings.json
            IHost host = Host.CreateDefaultBuilder(args).Build();
            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

            String authInstance = config.GetSection("AzureAd").GetValue<string>("Instance") ?? "";
            String authTenantId = config.GetSection("AzureAd").GetValue<string>("TenantId") ?? "";
            String authClientId = config.GetSection("AzureAd").GetValue<string>("ClientId") ?? "";
            String authClientSecret = "";
            String appserviceUri = config.GetValue<string>("AppServiceUri") ?? "";
            Uri appServiceSiteUri = new Uri(appserviceUri);
            String appserviceAuthClientId = config.GetValue<string>("AppServiceAuthClientID") ?? "";
            

            foreach (ConfigurationSection configSection in config.GetSection("AzureAd").GetSection("ClientCredentials").GetChildren())
            {
                String _authClientSecret = configSection.GetValue<String>("ClientSecret") ?? "";
                if (String.IsNullOrEmpty(_authClientSecret) == false)
                {
                    authClientSecret = _authClientSecret;
                }
            }


            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                // build uri: /{tenant}/oauth2/v2.0/token
                Uri OAuthUri = new Uri(authInstance + "" + authTenantId + "/oauth2/v2.0/token");

                // build POST contents
                String contentString = $"client_id={authClientId}&scope={appserviceAuthClientId}%2F.default" // https%3A%2F%2Fgraph.microsoft.com%2F.default
                    + $"&client_secret={authClientSecret}&grant_type=client_credentials";
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

                // request for AppServiceAuthentication (direct access with Authorization Header)
                // https://learn.microsoft.com/azure/app-service/configure-authentication-customize-sign-in-out#client-directed-sign-in
                request = new HttpRequestMessage(HttpMethod.Get, appserviceUri);
                request.Headers.Add("Authorization", $"Bearer {accessToken}");
                response = await httpClient.SendAsync(request);

                responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }
    }
}