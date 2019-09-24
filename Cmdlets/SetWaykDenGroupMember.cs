using System;
using System.Management.Automation;
using Newtonsoft.Json;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Set, "WaykDenGroupMember")]
    public class SetWaykDenGroupMember : RestApiCmdlet
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
                this.DenRestAPIController.PutUserToGroup(data, this.GroupID);
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }
    }
}
