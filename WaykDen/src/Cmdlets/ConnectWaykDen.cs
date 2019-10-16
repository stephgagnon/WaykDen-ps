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
        private const string DEN_SERVER_URL = "/health";
        [Parameter(HelpMessage = "WaykDen server API key.")]
        public string ApiKey {get; set;} = string.Empty;
        [Parameter(HelpMessage = "WaykDen server external URL.")]
        public string ServerUrl {get; set;} = string.Empty;
        protected async override void ProcessRecord()
        {
            try
            {
                DenConfig config = null;
                if(string.IsNullOrEmpty(this.ServerUrl) || string.IsNullOrEmpty(this.ApiKey))
                {
                    config = this.DenConfigController.GetConfig();
                }

                if(config != null)
                {
                    this.ServerUrl = config.DenServerConfigObject.ExternalUrl;
                    this.ApiKey = config.DenServerConfigObject.ApiKey;
                }

                Task<bool> okTask = this.TestDenServerRoute();
                okTask.Wait(10000);
                if(await okTask)
                {
                    Environment.SetEnvironmentVariable(DEN_API_KEY_ENV, this.ApiKey);
                    Environment.SetEnvironmentVariable(DEN_SERVER_URL_ENV, this.ServerUrl);
                    this.WriteObject($"Success! Server URL : {this.ServerUrl}");
                }
                else
                {
                    throw new Exception($"Having trouble reaching {this.ServerUrl}");
                }
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }

        private async Task<bool> TestDenServerRoute()
        {
            string url = $"{this.ServerUrl}{DEN_SERVER_URL}";
            using(var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    var response = await httpClient.SendAsync(request);
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
        }
    }
}