using System;
using System.Management.Automation;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
{
    [Cmdlet("Sync", "WaykDenUser")]
    public class SyncWaykDenUser : RestApiCmdlet
    {
        protected override void ProcessRecord()
        {
            this.DenRestAPIController.PostUser("sync");
        }
    }
}