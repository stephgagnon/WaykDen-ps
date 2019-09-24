using System;
using System.Management.Automation;
using System.Security;
using Newtonsoft.Json;
using WaykDen.Models;
using WaykDen.Utils;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Set, "WaykDenUser")]
    public class SetWaykDenUser : RestApiCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "WaykDen User ID.")]
        public string UserID { get; set; }

        [Parameter(HelpMessage = "WaykDen username.")]
        public string Username { get; set; }

        [Parameter(HelpMessage = "WaykDen User password.")]
        public string Password
        {
            get => SecureStringUtils.ToString(SafePassword);
            set => this.SafePassword = SecureStringUtils.FromString(value);
        }

        public SecureString SafePassword { get; set; }

        [Parameter(HelpMessage = "WaykDen Name of the user.")]
        public string Name { get; set; }

        [Parameter(HelpMessage = "WaykDen Email for the user.")]
        public string Email { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (!string.IsNullOrEmpty(this.Password))
                {
                    string dataPassword = JsonConvert.SerializeObject(new ByPasswordObject { password = this.Password },
                              Formatting.None,
                              new JsonSerializerSettings
                              {
                                  NullValueHandling = NullValueHandling.Ignore
                              });
                    this.DenRestAPIController.PutSetUserPassword(dataPassword, this.UserID);
                }

                if (!string.IsNullOrEmpty(Username) || !string.IsNullOrEmpty(Name) || !string.IsNullOrEmpty(Email))
                {
                    string data = JsonConvert.SerializeObject(new BySerialUserObject { username = this.Username, name = this.Name, email = this.Email, user_id = this.UserID  },
                           Formatting.None,
                           new JsonSerializerSettings
                           {
                               NullValueHandling = NullValueHandling.Ignore
                           });
                    this.DenRestAPIController.PutSetUserUpdate(data);
                }
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }
    }
}
