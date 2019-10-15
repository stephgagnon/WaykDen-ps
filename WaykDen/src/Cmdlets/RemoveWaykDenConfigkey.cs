using System;
using System.Management.Automation;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "WaykDenConfigKey")]
    public class RemoveWaykDenConfigKey: WaykDenConfigCmdlet
    {
        protected override void ProcessRecord()
        {
            try
            {
                this.DenConfigController.RemoveConfigKey(this.Key);
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }
    }
}