using System;
using System.Threading;
using System.Management.Automation;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
{
    [Cmdlet("Stop", "DevolutionsJet")]
    public class StopDevolutionsJet : baseCmdlet
    {
        private DenServicesController denServicesController;
        private ManualResetEvent mre = new ManualResetEvent(false);
        protected async override void ProcessRecord()
        {
            this.denServicesController = new DenServicesController(this.SessionState.Path.CurrentLocation.Path);
            this.denServicesController.OnError += this.OnError;
            await this.denServicesController.StopDevolutionsJet();
        }
    }
}