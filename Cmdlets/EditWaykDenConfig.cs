using System;
using System.Management.Automation;

namespace WaykDen.Cmdlets
{
    [Cmdlet("Edit", "WaykDenConfigKey")]
    public class EditWaykDenConfigKey: WaykDenConfigCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "New key to encrypt or decrypt configuration database.")]
        public string NewKey {get; set;}
        protected override void ProcessRecord()
        {
            try
            {
                this.DenConfigController.ChangeConfigKey(this.NewKey, this.Key);
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }
    }
}