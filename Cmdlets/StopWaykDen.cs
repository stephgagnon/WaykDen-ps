using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Management.Automation;
using WaykPS.Controllers;

namespace WaykPS.Cmdlets
{
    [Cmdlet("Stop", "WaykDen")]
    public class StopWaykDen : baseCmdlet
    {
        private DenServicesController denServicesController;
        private ManualResetEvent mre = new ManualResetEvent(false);
        private Mutex mutex = new Mutex();
        private ProgressRecord record;
        private int servicesCount = 0;
        protected async override void ProcessRecord()
        {
            try
            {
                this.denServicesController = new DenServicesController(this.SessionState.Path.CurrentLocation.Path);
                this.denServicesController.OnLog += this.OnLog;
                this.denServicesController.OnError += this.OnError;
                List<string> runningContainers = await this.denServicesController.GetRunningContainer();
                this.servicesCount = runningContainers.Count;
                Task stop = this.denServicesController.StopWaykDen(runningContainers);
                stop.Wait();
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }

        protected override void OnLog(string message)
        {
            ProgressRecord r = new ProgressRecord(1, "WaykDen", message);
            r.PercentComplete = this.servicesCount != 0 ? (this.servicesCount - this.denServicesController.ServicesCount) * 100 / this.servicesCount : 100;

            lock(this.mutex)
            {
                this.record = r;
                this.mre.Set();
            }
        }
    }
}