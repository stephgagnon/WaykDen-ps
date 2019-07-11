using System;
using System.Collections.Generic;
using System.Management.Automation;
using WaykPS.Controllers;

namespace WaykPS.Cmdlets
{
    [Cmdlet("Restart", "WaykDen")]
    public class RestartWaykDen : baseCmdlet
    {
        private DenServicesController denServicesController;

        protected async override void ProcessRecord()
        {
            this.denServicesController = new DenServicesController(this.SessionState.Path.CurrentLocation.Path);
            this.denServicesController.OnLog += this.OnLog;
            this.denServicesController.OnError += this.OnError;
            List<string> runningContainers = await this.denServicesController.GetRunningContainer();
            await this.denServicesController.StopWaykDen(runningContainers);
            await this.denServicesController.StartWaykDen();
        }
    }
}