using System;
using System.Management.Automation;
using Newtonsoft.Json;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "WaykDenGroupMember")]
    public class RemoveWaykDenGroupMember : RestApiCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "Wayk Den group ID.")]
        public string GroupID { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Wayk Den user ID.")]
        public string UserID { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                string data = JsonConvert.SerializeObject(new ByUserIDObject { user_id = this.UserID });
                this.DenRestAPIController.DeleteUserFromGroup(this.GroupID, data).Wait();
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }
    }
}
