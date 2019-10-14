using System;
using System.Management.Automation;
using Newtonsoft.Json;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Set, "WaykDenRoleMember")]
    public class SetWaykDenRoleMember : RestApiCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "Wayk Den User ID.")]
        public string UserID { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Wayk Den Role name from the group.")]
        public string RoleName { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                string data = JsonConvert.SerializeObject(new ByRoleName { role_name = this.RoleName });
                this.DenRestAPIController.PutRoleMember(data, this.UserID);
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }
    }
}
