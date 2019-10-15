using System;
using System.Management.Automation;
using WaykDen.Utils;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "WaykDenRole")]
    public class GetWaykDenRole : RestApiCmdlet
    {
        [Parameter(HelpMessage = "Wayk Den role ID.")]
        public string RoleID { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                string parameter = this.RoleID;
                string result = this.DenRestAPIController.GetRoles(parameter).Result;

                if (!string.IsNullOrEmpty(result))
                {
                    Role[] roles = { };
                    if (JsonUtils.IsArrayJsonObject(result))
                    {
                        roles = this.DenRestAPIController.DeserializeString<Role[]>(result);
                    }
                    else if (JsonUtils.IsSingleJsonObject(result))
                    {
                        roles = new Role[] { this.DenRestAPIController.DeserializeString<Role>(result) };
                    }

                    foreach (Role role in roles)
                    {
                        this.WriteObject(role.ToRoleObject, true);
                    }
                }
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }
    }
}
