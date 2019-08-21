using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Management.Automation;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
{
    [Cmdlet("Stop", "WaykDen")]
    public class StopWaykDen : WaykDenConfigCmdlet
    {
        private DenServicesController denServicesController;
        private int servicesCount = 0;
        private int stoppedServices = 0;
        protected async override void ProcessRecord()
        {
            try
            {
                this.denServicesController = new DenServicesController(this.Path, this.DenConfigController);
                this.denServicesController.OnLog += this.OnLog;
                this.denServicesController.OnError += this.OnError;
                List<string> runningContainers = await this.denServicesController.GetRunningContainers();
                this.servicesCount = runningContainers.Count;
                Task stop = this.denServicesController.StopWaykDen(runningContainers);
                while(!stop.IsCompleted && !stop.IsCanceled)
                {
                    mre.WaitOne();
                    lock(this.mutex)
                    {
                        this.WriteProgress(this.record);
                    }
                }
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }

        protected override void OnLog(string message)
        {
            ProgressRecord r = new ProgressRecord(1, "WaykDen", message);
            r.PercentComplete = this.stoppedServices / this.servicesCount * 100;

            lock(this.mutex)
            {
                this.record = r;
                this.mre.Set();
            }
        }
    }
}