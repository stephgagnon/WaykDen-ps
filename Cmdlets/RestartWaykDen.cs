using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
{
    [Cmdlet("Restart", "WaykDen")]
    public class RestartWaykDen : WaykDenConfigCmdlet
    {
        private DenServicesController denServicesController;

        protected override void ProcessRecord()
        {
            try
            {
                this.denServicesController = new DenServicesController(this.Path, this.Key);
                this.denServicesController.OnLog += this.OnLog;
                this.denServicesController.OnError += this.OnError;

                var cmdStop = new StopWaykDen();
                cmdStop.Invoke();
                var cmdStart = new StartWaykDen();
                cmdStart.Invoke();
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