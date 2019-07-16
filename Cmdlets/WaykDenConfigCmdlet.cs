using System;
using System.Management.Automation;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
{
    public class WaykDenConfigCmdlet : BaseCmdlet
    {
        private const string WAYK_DEN_HOME = "WAYK_DEN_HOME";
        private const string WAYK_DEN_CONFIG_KEY = "WAYK_DEN_CONFIG_KEY";
        protected string Path {get; set;}
        protected DenConfigController DenConfigController {get; set;}
        [Parameter(HelpMessage = "Key to encrypt or decrypt configuration database.")]
        public string Key {get; set;}
        protected override void BeginProcessing()
        {
            try
            {
                this.Path = Environment.GetEnvironmentVariable(WAYK_DEN_HOME);
                if(string.IsNullOrEmpty(this.Path))
                {
                    this.Path = this.SessionState.Path.CurrentLocation.Path;
                }

                if(string.IsNullOrEmpty(this.Key))
                {
                   this.Key  = Environment.GetEnvironmentVariable(WAYK_DEN_CONFIG_KEY);
                }

                this.DenConfigController = new DenConfigController(this.Path, this.Key);
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }
    }
}