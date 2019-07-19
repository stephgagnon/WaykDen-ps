using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
{
    public class RestApiCmdlet : BaseCmdlet
    {
        private const string DEN_API_KEY_ENV = "DEN_API_KEY";
        private const string DEN_SERVER_URL_ENV = "DEN_SERVER_URL";
        private const string API_KEY_FIELD = "ApiKey";
        private const string SERVER_URL_FIELD = "Server URL";
        [Parameter(HelpMessage = "WaykDen server API key.")]
        public string ApiKey {get; set;} = string.Empty;
        [Parameter(HelpMessage = "WaykDen server external URL.")]
        public string ServerUrl {get; set;} = string.Empty;
        protected DenRestAPIController DenRestAPIController {get; set;}
        protected override void BeginProcessing()
        {
            if(string.IsNullOrEmpty(this.ApiKey))
            {
                this.ApiKey = Environment.GetEnvironmentVariable(DEN_API_KEY_ENV);
            }

            if( string.IsNullOrEmpty(this.ServerUrl))
            {
                this.ServerUrl = Environment.GetEnvironmentVariable(DEN_SERVER_URL_ENV);
            }

            if(string.IsNullOrEmpty(this.ApiKey) || string.IsNullOrEmpty(this.ServerUrl))
            {
                Dictionary<string, PSObject> prompt =  this.Host.UI.Prompt
                (
                    "WaykDen cmdlet",
                    "Supply values for the following parameters:",
                    new Collection<FieldDescription>(){new FieldDescription(SERVER_URL_FIELD), new FieldDescription(API_KEY_FIELD)}
                );

                bool okApiKey = prompt.TryGetValue(API_KEY_FIELD, out PSObject objApi);
                bool okServerUrl = prompt.TryGetValue(SERVER_URL_FIELD, out PSObject objUrl);

                this.ApiKey = (string)objApi.BaseObject;
                this.ServerUrl = (string)objUrl.BaseObject;
            }

            if(string.IsNullOrEmpty(this.ApiKey) || string.IsNullOrEmpty(this.ServerUrl))
            {
                this.OnError(new Exception("No API key or server URL were provided"));
            }

            this.ServerUrl = this.ServerUrl.TrimEnd('/');
            this.DenRestAPIController = new DenRestAPIController(this.ApiKey, this.ServerUrl);
            this.DenRestAPIController.OnError += this.OnError;
        }
    }
}