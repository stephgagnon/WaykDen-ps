using System;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
{
    [Cmdlet("Start", "WaykDen")]
    public class StartWaykDen : baseCmdlet
    {
        private DenServicesController denServicesController;
        protected override void ProcessRecord()
        {
            try
            {
                this.denServicesController = new DenServicesController(this.SessionState.Path.CurrentLocation.Path);
                this.denServicesController.OnLog += this.OnLog;
                this.denServicesController.OnError += this.OnError;
                Task<bool> start = this.denServicesController.StartWaykDen();

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
            r.PercentComplete = this.denServicesController.RunningDenServices.Count * 100 / 6;

            lock(this.mutex)
            {
                this.record = r;
                this.mre.Set();
            }
        }
    }
}