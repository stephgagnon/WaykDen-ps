using System;
using System.Threading;
using System.Management.Automation;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
{
    [Cmdlet("Start", "DevolutionsJet")]
    public class StartDevolutionsJet : WaykDenConfigCmdlet
    {
        private DenServicesController denServicesController;
        protected async override void ProcessRecord()
        {
            this.denServicesController = new DenServicesController(this.Path, this.DenConfigController);
            this.denServicesController.OnError += this.OnError;
            this.denServicesController.OnLog += this.OnLog;

            await this.denServicesController.StartDevolutionsJet();
        }
    }
}