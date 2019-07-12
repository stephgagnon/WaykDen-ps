using System;
using System.Management.Automation;
using System.Threading.Tasks;
using WaykDen.Controllers;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "WaykDenLicense")]
    public class GetWaykDenLicense : baseCmdlet
    {
        [Parameter(HelpMessage = "A License ID.")]
        public string ID {get; set;}
        public GetWaykDenLicense()
        {   
        }

        protected async override void ProcessRecord()
        {
            try
            {
                DenRestAPIController denRestAPIController = new DenRestAPIController(this.SessionState.Path.CurrentLocation.Path);
                denRestAPIController.OnError += this.OnError;
                string parameter = null;
                if(!string.IsNullOrEmpty(this.ID))
                {
                    parameter = $"/{this.ID}";
                }
                Task<string> licensesString = denRestAPIController.GetLicenses(parameter);
                licensesString.Wait();

                string res = await licensesString;

                if(res.StartsWith('['))
                {
                    var licenses = denRestAPIController.DeserializeString<License[]>(res);
                    
                    foreach(License license in licenses)
                    {
                        this.WriteObject(license.ToLicenseObject(), true);
                    }
                }
                else
                {
                    var license = denRestAPIController.DeserializeString<License>(res);
                    this.WriteObject(license?.ToLicenseObject());
                }
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }
    }
}