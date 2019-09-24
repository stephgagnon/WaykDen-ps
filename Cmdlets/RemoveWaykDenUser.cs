using System;
using System.Management.Automation;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "WaykDenUser")]
    public class RemoveWaykDenUser : RestApiCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "WaykDen user ID.")]
        public string ID { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                this.DenRestAPIController.DeleteUser(this.ID).Wait();
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }
    }
}
