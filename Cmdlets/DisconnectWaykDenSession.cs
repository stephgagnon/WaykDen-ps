using Newtonsoft.Json;
using System;
using System.Management.Automation;
using System.Threading.Tasks;
using WaykPS.Controllers;
using WaykPS.RestAPI;

namespace WaykPS.Cmdlets
{
    [Cmdlet("Disconnect", "WaykDenSession")]
    public class DisconnectWaykDenSession : baseCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "ID of a session.")]
        public string SessionID {get; set;} = string.Empty;
        
        public DisconnectWaykDenSession()
        {
        }

        protected override void ProcessRecord()
        {
            DenRestAPIController denRestAPIController = new DenRestAPIController(this.SessionState.Path.CurrentLocation.Path);
            denRestAPIController.OnError += this.OnError;
            denRestAPIController.PostSession($"{this.SessionID}/disconnect");
        }
    }
}