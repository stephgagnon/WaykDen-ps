using System;
using System.Management.Automation;
using Newtonsoft.Json;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Set, "WaykDenRoleGroup")]
    public class SetWaykDenRoleGroup : RestApiCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "Wayk Den group ID.")]
        public string GroupID { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Wayk Den Role name from the group.")]
        public string RoleName { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                string data = JsonConvert.SerializeObject(new ByRoleName { role_name = this.RoleName });
                this.DenRestAPIController.PutRoleToGroup(data, this.GroupID);
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }
    }
}
