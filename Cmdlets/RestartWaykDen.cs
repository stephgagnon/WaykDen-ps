using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
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
            Task stop = this.denServicesController.StopWaykDen(runningContainers);
            stop.Wait();
            Task start = this.denServicesController.StartWaykDen();
            start.Wait();
        }
    }
}