using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace azure_appservice_authentication_msid_sample
{
    internal class Program
    {
        // AzureAd information
        private String authInstance { get; }
        private String authTenantId { get; }
        private String authClientId { get; }
        private String authClientSecret { get; }

        // target AppService
        private Uri appServiceSiteUri { get; }
        private String appserviceAuthClientId { get; }


        Program() {
            // read config from appsettings.json
            IHost host = Host.CreateDefaultBuilder().Build();
            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

            authInstance = config.GetSection("AzureAd").GetValue<string>("Instance") ?? "";
            authTenantId = config.GetSection("AzureAd").GetValue<string>("TenantId") ?? "";
            authClientId = config.GetSection("AzureAd").GetValue<string>("ClientId") ?? "";
            authClientSecret = "";
            foreach (ConfigurationSection configSection in config.GetSection("AzureAd").GetSection("ClientCredentials").GetChildren())
            {
                String _authClientSecret = configSection.GetValue<String>("ClientSecret") ?? "";
                if (String.IsNullOrEmpty(_authClientSecret) == false)
                {
                    authClientSecret = _authClientSecret;
                }
            }

            String _appserviceUriString = config.GetValue<string>("AppServiceUri") ?? "";
            appServiceSiteUri = new Uri(_appserviceUriString);
            appserviceAuthClientId = config.GetValue<string>("AppServiceAuthClientID") ?? "";
        }


        static async Task Main(string[] args)
        {
            Console.WriteLine("Azure App Service Authentication access sample.");

            Program p = new Program();

            //ICallAuthProviderSample authClient = new ExampleHttpClient(p.authInstance, p.appserviceAuthClientId);
            ICallAuthProviderSample authClient = new ExampleMSAL(p.authInstance, p.appserviceAuthClientId);
            String token = await authClient.getToken(p.authTenantId, p.authClientId, p.authClientSecret);
            String contents = await authClient.accessAppService(p.appServiceSiteUri, token);

            Console.WriteLine(contents);
        }
    }
}