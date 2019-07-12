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
            try
            {
                this.denServicesController = new DenServicesController(this.SessionState.Path.CurrentLocation.Path);
                this.denServicesController.OnLog += this.OnLog;
                this.denServicesController.OnError += this.OnError;
                List<string> runningContainers = await this.denServicesController.GetRunningContainers();

                Task stop = this.denServicesController.StopWaykDen(runningContainers);

                while(!stop.IsCompleted && !stop.IsCanceled)
                {
                    mre.WaitOne();
                    lock(this.mutex)
                    {
                        this.WriteProgress(this.record);
                    }
                }

                Task start = this.denServicesController.StartWaykDen();

                while(!start.IsCompleted && !start.IsCanceled)
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

            lock(this.mutex)
            {
                this.record = r;
                this.mre.Set();
            }
        }
    }
}