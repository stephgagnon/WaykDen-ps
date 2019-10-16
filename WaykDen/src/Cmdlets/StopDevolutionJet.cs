using System;
using System.Threading;
using System.Management.Automation;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
{
    [Cmdlet("Stop", "DevolutionsJet")]
    public class StopDevolutionsJet : WaykDenConfigCmdlet
    {
        private DenServicesController denServicesController;
        protected async override void ProcessRecord()
        {
            this.denServicesController = new DenServicesController(this.Path);
            this.denServicesController.OnError += this.OnError;
            await this.denServicesController.StopDevolutionsJet();
        }
    }
}