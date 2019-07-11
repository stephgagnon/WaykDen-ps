using System;
using System.Management.Automation;
using WaykPS.Controllers;

namespace WaykPS.Cmdlets
{
    [Cmdlet("Sync", "WaykDenUser")]
    public class SyncWaykDenUser : baseCmdlet
    {
        public SyncWaykDenUser()
        {   
        }

        protected override void ProcessRecord()
        {
            DenRestAPIController denRestAPIController = new DenRestAPIController(this.SessionState.Path.CurrentLocation.Path);
            denRestAPIController.OnError += this.OnError;
            denRestAPIController.PostUser("sync");
        }
    }
}