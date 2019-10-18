using System;
using System.Management.Automation;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "WaykDenRoleMember")]
    public class RemoveWaykDenRoleMember : RestApiCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "WaykDen user ID.")]
        public string UserID { get; set; }

        protected override void ProcessRecord()
        {
            this.DenRestAPIController.DeleteUserFromRole(this.UserID);
        }
    }
}
