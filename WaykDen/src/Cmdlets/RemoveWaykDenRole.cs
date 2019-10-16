using System;
using System.Management.Automation;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "WaykDenRole")]
    public class RemoveWaykDenRole : RestApiCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "WaykDen role ID.")]
        public string RoleID { get; set; }

        protected override void ProcessRecord()
        {
            this.DenRestAPIController.DeleteRole(this.RoleID).Wait();
        }
    }
}
