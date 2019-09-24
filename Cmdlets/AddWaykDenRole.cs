using System;
using System.Management.Automation;
using Newtonsoft.Json;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, "WaykDenRole")]
    public class AddWaykDenRole : RestApiCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "WaykDen role name.")]
        public string RoleName { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                string data = JsonConvert.SerializeObject(new ByNameObject { name = this.RoleName });
                string post = this.DenRestAPIController.PostCreateRole(data);
                if (!string.IsNullOrEmpty(post))
                {
                    this.WriteObject(new RoleObject { ID = post, Name = this.RoleName});
                }
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }
    }
}
