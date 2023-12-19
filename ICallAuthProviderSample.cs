using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace azure_appservice_authentication_msid_sample
{
    internal interface ICallAuthProviderSample
    {
        public Task<String> getToken(String TenantId, String ClientId, String Secret);
        public Task<String> accessAppService(Uri AppServiceUri, String accessToken);
    }
}
