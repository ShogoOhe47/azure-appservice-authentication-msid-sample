using System.Text;

namespace azure_appservice_authentication_msid_sample
{
    internal class ExampleHttpClientSharedCode
    {

        public static async Task<String> accessAppService(HttpClient httpClient, Uri AppServiceUri, String accessToken)
        {
            // request for AppServiceAuthentication (direct access with Authorization Header)
            // https://learn.microsoft.com/azure/app-service/configure-authentication-customize-sign-in-out#client-directed-sign-in
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, AppServiceUri);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            HttpResponseMessage response = await httpClient.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseBody);


            // request for AppServiceAuthentication (using X-ZUMO-AUTH header)
            // https://learn.microsoft.com/azure/app-service/configure-authentication-customize-sign-in-out#client-directed-sign-in
            Uri appserviceUriLoginAAD = new Uri(AppServiceUri + ".auth/login/aad");
            String contentString = $"{{\"access_token\":\"{accessToken}\"}}";
            StringContent content = new StringContent(contentString, Encoding.UTF8, @"application/json");

            request = new HttpRequestMessage(HttpMethod.Post, appserviceUriLoginAAD);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            response = await httpClient.SendAsync(request);

            responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseBody);

            return responseBody;
        }
    }
}