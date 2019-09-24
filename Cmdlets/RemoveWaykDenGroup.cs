using System;
using System.Management.Automation;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "WaykDenGroup")]
    public class RemoveWaykDenGroup : RestApiCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "WaykDen group ID.")]
        public string GroupID { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                this.DenRestAPIController.DeleteGroup(this.GroupID).Wait();
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }
    }
}
