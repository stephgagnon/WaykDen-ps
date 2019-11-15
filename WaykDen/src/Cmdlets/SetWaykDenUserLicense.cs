using Newtonsoft.Json;
using System.Management.Automation;
using WaykDen.Models;
using System.Threading.Tasks;
using System;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Set, "WaykDenUserLicense"), CmdletBinding(DefaultParameterSetName="ByUserID")]
    public class SetWaykDenUserLicense : RestApiCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "ByUserID", HelpMessage = "ID of a user.")]
        public string UserID {get; set;}
        [Parameter(Mandatory = true, ParameterSetName = "ByUsername", HelpMessage = "Username of a user.")]
        public string Username {get; set;}
        [Parameter(ParameterSetName = "ByUsername", HelpMessage = "License serial")]
        [Parameter(ParameterSetName = "ByUserID", HelpMessage = "License serial.")]
        public string Serial {get; set;}
        [Parameter(ParameterSetName = "ByUsername", HelpMessage = "ID of a license.")]
        [Parameter(ParameterSetName = "ByUserID", HelpMessage = "ID of a license.")]
        public string LicenseID {get; set;}
        protected async override void ProcessRecord()
        {
            string data = string.Empty;
            var bySerial = !string.IsNullOrEmpty(this.Serial);
            var byUsername = this.ParameterSetName == "ByUsername";

            try
            {
                Task<User> findUser = this.DenRestAPIController.GetUserByNameOrUsername(byUsername ? this.Username : this.UserID, byUsername);
                findUser.Wait();
                User user = await findUser;
                
                if (user != null) {
                    Task<License> findLicense = this.DenRestAPIController.GetLicenseByIdOrSerial(bySerial ? this.Serial : this.LicenseID, bySerial);
                    findLicense.Wait();
                    License license = await findLicense;

                    if (license != null) {
                        var settings = new JsonSerializerSettings();
                        settings.NullValueHandling = NullValueHandling.Ignore;
                        data = JsonConvert.SerializeObject(new User{license_id = license._id.oid}, settings);
                        this.DenRestAPIController.PatchUser(user.id, data);
                    } else {
                        this.WriteWarning("License not found");
                    }
                } else {
                    this.WriteWarning("User not found");
                }
                
            } catch(Exception e) {
                this.OnError(e);
            }
        }
    }
}