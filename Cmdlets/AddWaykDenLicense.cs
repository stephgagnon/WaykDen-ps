using Newtonsoft.Json;
using System.Management.Automation;
using WaykDen.Controllers;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, "WaykDenLicense")]
    public class AddWaykDenLicense : RestApiCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "WaykDen license serial.")]
        public string Serial {get; set;}

        protected override void ProcessRecord()
        {
            string data = JsonConvert.SerializeObject(new BySerialObject{serial_number = this.Serial});
            string post = this.DenRestAPIController.PostLicense(null, data);
            this.WriteObject(post);
        }
    }
}