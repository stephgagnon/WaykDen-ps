using System.Management.Automation;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
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