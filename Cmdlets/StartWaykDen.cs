using System;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using WaykPS.Controllers;

namespace WaykPS.Cmdlets
{
    [Cmdlet("Start", "WaykDen")]
    public class StartWaykDen : baseCmdlet
    {
        private DenServicesController denServicesController;
        private ManualResetEvent mre = new ManualResetEvent(false);
        private Mutex mutex = new Mutex();
        private ProgressRecord record;
        protected override void ProcessRecord()
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