using System;
using System.Management.Automation;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
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