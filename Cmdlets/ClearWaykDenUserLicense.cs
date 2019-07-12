using Newtonsoft.Json;
using System.Management.Automation;
using WaykDen.Controllers;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Clear, "WaykDenUserLicense"), CmdletBinding(DefaultParameterSetName="ByUserID")]
    public class ClearWaykDenUserLicense : baseCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "ByUserID", HelpMessage = "Delete the license of a user using his user ID.")]
        public string UserID {get; set;}
        [Parameter(Mandatory = true, ParameterSetName = "ByUsername", HelpMessage = "Delete the license of a user using his username.")]
        public string Username {get; set;}
        
        public ClearWaykDenUserLicense()
        {
        }

        protected async override void ProcessRecord()
        {
            DenRestAPIController denRestAPIController = new DenRestAPIController(this.SessionState.Path.CurrentLocation.Path);
            denRestAPIController.OnError += this.OnError;
            string data = string.Empty;
            if(this.ParameterSetName == "ByUsername")
            {
                data = JsonConvert.SerializeObject(new ByUsernameObject{username = this.Username});
                denRestAPIController.PutUser(data);
            }
            else
            {
                await denRestAPIController.DeleteUserLicense(this.UserID);
            }
        }
    }
}