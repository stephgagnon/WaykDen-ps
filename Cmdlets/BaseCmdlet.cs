using System;
using System.Threading;
using System.Management.Automation;

namespace WaykDen.Cmdlets
{
    public class baseCmdlet : PSCmdlet
    {
        protected ManualResetEvent mre = new ManualResetEvent(false);
        protected Mutex mutex = new Mutex();
        protected ProgressRecord record;
        protected virtual void OnError(Exception e)
        {
            ErrorRecord error = new ErrorRecord(e, e.StackTrace, ErrorCategory.InvalidData, e.Data);
            this.WriteWarning(e.Message);
            this.ThrowTerminatingError(error);
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