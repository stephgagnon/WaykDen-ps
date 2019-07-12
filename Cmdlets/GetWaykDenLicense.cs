using System;
using System.Management.Automation;
using System.Threading.Tasks;
using WaykDen.Controllers;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "WaykDenLicense")]
    public class GetWaykDenLicense : RestApiCmdlet
    {
        [Parameter(HelpMessage = "A License ID.")]
        public string ID {get; set;}
        protected async override void ProcessRecord()
        {
            try
            {
                string parameter = null;
                if(!string.IsNullOrEmpty(this.ID))
                {
                    parameter = $"/{this.ID}";
                }
                Task<string> licensesString = this.DenRestAPIController.GetLicenses(parameter);
                licensesString.Wait();

                string res = await licensesString;

                if(res.StartsWith('['))
                {
                    var licenses = this.DenRestAPIController.DeserializeString<License[]>(res);
                    
                    foreach(License license in licenses)
                    {
                        this.WriteObject(license.ToLicenseObject(), true);
                    }
                }
                else
                {
                    var license = this.DenRestAPIController.DeserializeString<License>(res);
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