using System.Management.Automation;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
{
    [Cmdlet("Disconnect", "WaykDenSession")]
    public class DisconnectWaykDenSession : RestApiCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "ID of a session.")]
        public string SessionID {get; set;} = string.Empty;

        protected override void ProcessRecord()
        {
            this.DenRestAPIController.PostSession($"{this.SessionID}/disconnect");
        }
    }
}