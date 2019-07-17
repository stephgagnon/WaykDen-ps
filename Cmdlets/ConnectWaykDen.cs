using System;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WaykDen.Controllers;
using WaykDen.Models;
using WaykDen.Models.Services;

namespace WaykDen.Cmdlets
{
    [Cmdlet("Connect", "WaykDen")]
    public class ConnectWaykDen : WaykDenConfigCmdlet
    {
        private const string DEN_API_KEY_ENV = "DEN_API_KEY";
        private const string DEN_SERVER_URL_ENV = "DEN_SERVER_URL";
        private const string DEN_LUCID_URL = "/lucid/health";
        private const string DEN_SERVER_URL = "/health";
        protected async override void ProcessRecord()
        {
            try
            {
                DenConfig config = this.DenConfigController.GetConfig();
                DenServicesController denServicesController = new DenServicesController(this.Path, this.Key);
                Environment.SetEnvironmentVariable(DEN_API_KEY_ENV, config?.DenServerConfigObject.ApiKey);
                Environment.SetEnvironmentVariable(DEN_SERVER_URL_ENV, config?.DenServerConfigObject.ExternalUrl);
                Task<bool> okTask = this.TestRoute($"{config?.DenServerConfigObject.ExternalUrl}{DEN_LUCID_URL}", config.DenLucidConfigObject.ApiKey);
                okTask.Wait();

                DenTraefikService traefik= null;
                if(!await okTask)
                {
                    traefik = new DenTraefikService(denServicesController);
                    bool ok = await traefik.CurlTraefikConfig();
                    if(!ok)
                    {
                        await traefik.CurlTraefikConfig();
                    }
                }

                okTask = this.TestRoute($"{config.DenServerConfigObject.ExternalUrl}{DEN_SERVER_URL}", config.DenServerConfigObject.ApiKey);
                if(!await okTask)
                {
                    throw new Exception("Traefik routes are not set up. Try to restart WaykDen.");
                }
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }

        private async Task<bool> TestRoute(string url, string apiKey)
        {
            using(var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    var response = await httpClient.SendAsync(request);
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
        }
    }
}