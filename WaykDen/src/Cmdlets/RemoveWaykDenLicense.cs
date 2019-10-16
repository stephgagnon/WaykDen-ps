using Newtonsoft.Json;
using System;
using System.Management.Automation;
using System.Threading.Tasks;
using WaykDen.Controllers;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "WaykDenLicense")]
    public class RemoveWaykDenLicense : RestApiCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "ID of a WaykDen license.")]
        public string LicenseID {get; set;}
        protected async override void ProcessRecord()
        {
            await this.DenRestAPIController.DeleteLicense(this.LicenseID);
        }
    }
}