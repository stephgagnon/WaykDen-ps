using System;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
{
    [Cmdlet("Start", "WaykDen")]
    public class StartWaykDen : WaykDenConfigCmdlet
    {
        private Exception error;
        private DenServicesController denServicesController;
        private bool started = false;
        protected override void ProcessRecord()
        {
            try
            {
                this.denServicesController = new DenServicesController(this.Path, this.DenConfigController);
                this.denServicesController.OnLog += this.OnLog;
                this.denServicesController.OnError += this.OnError;
                Task<bool> start = this.denServicesController.StartWaykDen();
                this.started = true;

                while(!start.IsCompleted && !start.IsCanceled)
                {
                    this.mre.WaitOne();
                    lock(this.mutex)
                    {
                        if(this.record != null)
                        {
                            this.WriteProgress(this.record);
                            this.record = null;
                        }

                        if(this.error != null)
                        {
                            this.WriteError(new ErrorRecord(this.error, this.error.StackTrace, ErrorCategory.InvalidData, this.error.Data));
                            this.error = null;
                        }
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

        protected override void OnError(Exception e)
        {
            this.error = e;

            if(!started)
            {
                this.WriteError(new ErrorRecord(e, e.StackTrace, ErrorCategory.InvalidData, e.Data));
            }

            lock(this.mutex)
            {
                this.mre.Set();
            }
        }
    }
}