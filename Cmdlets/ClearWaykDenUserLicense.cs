using Newtonsoft.Json;
using System.Management.Automation;
using WaykDen.Controllers;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Clear, "WaykDenUserLicense"), CmdletBinding(DefaultParameterSetName="ByUserID")]
    public class ClearWaykDenUserLicense : RestApiCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "ByUserID", HelpMessage = "Delete the license of a user using his user ID.")]
        public string UserID {get; set;}
        [Parameter(Mandatory = true, ParameterSetName = "ByUsername", HelpMessage = "Delete the license of a user using his username.")]
        public string Username {get; set;}
        protected async override void ProcessRecord()
        {
            string data = string.Empty;
            if(this.ParameterSetName == "ByUsername")
            {
                data = JsonConvert.SerializeObject(new ByUsernameObject{username = this.Username});
                this.DenRestAPIController.PutUser(data);
            }
            else
            {
                await this.DenRestAPIController.DeleteUserLicense(this.UserID);
            }
        }
    }
}