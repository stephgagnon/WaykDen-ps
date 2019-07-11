using Newtonsoft.Json;
using System;
using System.Management.Automation;
using System.Threading.Tasks;
using WaykPS.Controllers;
using WaykPS.RestAPI;

namespace WaykPS.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "WaykDenLicense")]
    public class RemoveWaykDenLicense : baseCmdlet
    {
        private DenRestAPIController denRestAPIController {get; set;}
        [Parameter(Mandatory = true, HelpMessage = "ID of a WaykDen license.")]
        public string LicenseID {get; set;}
        
        public RemoveWaykDenLicense()
        {
        }

        protected async override void ProcessRecord()
        {
            DenRestAPIController denRestAPIController = new DenRestAPIController(this.SessionState.Path.CurrentLocation.Path);
            denRestAPIController.OnError += this.OnError;
            await denRestAPIController.DeleteLicense(this.LicenseID);
        }
    }
}