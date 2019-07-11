using System;
using System.Management.Automation;
using System.Threading.Tasks;
using WaykPS.Controllers;
using WaykPS.RestAPI;

namespace WaykPS.Cmdlets
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

                if(parameter == null)
                {
                    var licenses = denRestAPIController.DeserializeString<License[]>(await licensesString);
                    
                    foreach(License license in licenses)
                    {
                        this.WriteObject(license.ToLicenseObject(), true);
                    }
                }
                else 
                {
                    var user = denRestAPIController.DeserializeString<License>(await licensesString);
                    this.WriteObject(user?.ToLicenseObject(), true);
                }
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }
    }
}