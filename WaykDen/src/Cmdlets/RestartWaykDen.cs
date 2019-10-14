using System;
using System.Management.Automation;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
{
    [Cmdlet("Restart", "WaykDen")]
    public class RestartWaykDen : WaykDenConfigCmdlet
    {
        private const string STOP_WAYKDEN = "Stop-WaykDen";
        private const string START_WAYKDEN = "Start-WaykDen";
        private DenServicesController denServicesController;

        protected override void ProcessRecord()
        {
            try
            {
                this.denServicesController = new DenServicesController(this.Path, this.DenConfigController);
                this.denServicesController.OnLog += this.OnLog;
                this.denServicesController.OnError += this.OnError;

                PowerShell ps = PowerShell.Create();
                this.InvokeCmdLet(ps, "Import-Module", "Assembly", System.Reflection.Assembly.GetExecutingAssembly());
                this.InvokeCmdLet(ps, STOP_WAYKDEN);
                this.InvokeCmdLet(ps, START_WAYKDEN);
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

        private void InvokeCmdLet(PowerShell ps, string cmd, string parameterName = null, object value = null)
        {
            if(string.IsNullOrEmpty(parameterName))
            {
                ps.AddCommand(cmd, true);
            }
            else
            {
                ps.AddCommand(cmd, true).AddParameter(parameterName, value);
            }

            ps.Invoke(null, new PSInvocationSettings{RemoteStreamOptions = RemoteStreamOptions.AddInvocationInfo, Host = this.Host});
            ps.Commands.Clear();
        }
    }
}