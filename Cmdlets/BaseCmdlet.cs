using System;
using System.Management.Automation;

namespace WaykPS.Cmdlets
{
    public class baseCmdlet : PSCmdlet
    {
        protected virtual void OnError(Exception e)
        {
            ErrorRecord error = new ErrorRecord(e, e.StackTrace, ErrorCategory.InvalidData, e.Data);
            this.WriteWarning(e.Message);
            this.WriteError(error);
            this.StopProcessing();
        }

        protected virtual void OnLog(string message)
        {
            this.WriteProgress
            (
                new ProgressRecord(1, "WaykDen", message)
            );
        }
    }
}