using System;
using System.Management.Automation;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
{
    [Cmdlet("Add", "WaykDenConfigKey")]
    public class AddWaykDenConfigKey: WaykDenConfigCmdlet
    {
        protected override void ProcessRecord()
        {
            try
            {
                this.DenConfigController.AddConfigKey(this.Key);
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }
    }
}