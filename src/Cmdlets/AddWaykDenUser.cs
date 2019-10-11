using System;
using System.Management.Automation;
using System.Security;
using Newtonsoft.Json;
using WaykDen.Models;
using WaykDen.Utils;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, "WaykDenUser")]
    public class AddWaykDenUser : RestApiCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "WaykDen username.")]
        public string Username { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "WaykDen User password.")]
        public string Password
        {
            get => SecureStringUtils.ToString(SafePassword);
            set => this.SafePassword = SecureStringUtils.FromString(value);
        }

        [Parameter(HelpMessage = "WaykDen Name of the user.")]
        public string Name { get; set; }

        [Parameter(HelpMessage = "WaykDen Email of the user.")]
        public string Email { get; set; }

        public SecureString SafePassword { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                // We need to ignore the null value, or the database will set the empty field from the json
                string data = JsonConvert.SerializeObject(new BySerialUserObject { username = this.Username, password = this.Password, name = this.Name, email = this.Email },
                Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                string post = this.DenRestAPIController.PostCreateUser(data);
                if (!string.IsNullOrEmpty(post))
                {
                    this.WriteObject(new User { _id = new Oid { oid = post }, username = Username, email = this.Email, name = this.Name }.ToUserObject());
                }
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }
    }
}
