using Newtonsoft.Json;
using System;
using System.Management.Automation;
using System.Threading.Tasks;
using WaykPS.Controllers;
using WaykPS.RestAPI;

namespace WaykPS.Cmdlets
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
            string post = denRestAPIController.PostLicense(data);
            this.WriteObject(post);
        }
    }
}