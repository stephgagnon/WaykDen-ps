using System;
using System.Management.Automation;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Set, "WaykDenRoleMember")]
    public class SetWaykDenRoleMember : RestApiCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "Wayk Den User ID.")]
        public string UserID { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Wayk Den Role name from the group.")]
        public string RoleName { get; set; }

        protected async override void ProcessRecord()
        {
            try
            {
                Task<Role> findRole = this.DenRestAPIController.GetRoleByName(this.RoleName);
                findRole.Wait();
                Role role = await findRole;

                if (role != null) {
                    string data = JsonConvert.SerializeObject(new User { role_id = role._id.oid });
                    this.DenRestAPIController.PatchUser(this.UserID, data);
                } else {
                    this.WriteWarning("Role not found");
                }
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }
    }
}
