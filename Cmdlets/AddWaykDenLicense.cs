using Newtonsoft.Json;
using System.Management.Automation;
using WaykDen.Controllers;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, "WaykDenLicense")]
    public class AddWaykDenLicense : baseCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "WaykDen license serial.")]
        public string Serial {get; set;}
        public AddWaykDenLicense()
        {   
        }

        protected override void ProcessRecord()
        {
            DenRestAPIController denRestAPIController = new DenRestAPIController(this.SessionState.Path.CurrentLocation.Path);
            denRestAPIController.OnError += this.OnError;
            string data = JsonConvert.SerializeObject(new BySerialObject{serial_number = this.Serial});
            string post = denRestAPIController.PostLicense(null, data);
            this.WriteObject(post);
        }
    }
}