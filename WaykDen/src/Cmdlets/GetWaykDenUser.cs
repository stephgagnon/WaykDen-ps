using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using WaykDen.Controllers;
using WaykDen.Models;
using WaykDen.Utils;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "WaykDenUser")]
    [OutputType(new Type[]
    {
        typeof(UserObject)
    })]
    public class GetWaykDenUser : RestApiCmdlet
    {
        private enum UserGetOption
        {
            ByUsername,
            ByID
        }

        [Parameter(HelpMessage = "Wayk Den group ID.")]
        public string GroupID { get; set; }

        [Parameter]
        public string Username { get; set; }

        [Parameter(HelpMessage = "User ID.")]
        public string ID { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                string parameter = null;
            
                if(!string.IsNullOrEmpty(this.ID))
                {
                    parameter = this.ParameterBuilder(UserGetOption.ByID, !string.IsNullOrEmpty(this.GroupID));
                }
                else if(!string.IsNullOrEmpty(this.Username))
                {
                    parameter = this.ParameterBuilder(UserGetOption.ByUsername, !string.IsNullOrEmpty(this.GroupID));
                }

                string res;

                if (!string.IsNullOrEmpty(this.GroupID))
                {
                    res = this.DenRestAPIController.GetUserFromGroup(this.GroupID, parameter).Result;
                }
                else
                {
                    res = this.DenRestAPIController.GetUsers(parameter).Result;
                }

                if(JsonUtils.IsArrayJsonObject(res))
                {
                    var users = this.DenRestAPIController.DeserializeString<User[]>(res);
                    
                    foreach(User user in users)
                    {
                        this.WriteObject(user.ToUserObject(), true);
                    }
                }
                else if(JsonUtils.IsSingleJsonObject(res))
                {
                    var user = this.DenRestAPIController.DeserializeString<User>(res);
                    this.WriteObject(user?.ToUserObject());
                }
            }
            catch(Exception e)
            {
                this.OnError(e);
                return;
            }
        }

        private string ParameterBuilder(UserGetOption getOption, bool groupSearch = false)
        {
            if (getOption == UserGetOption.ByID)
            {
                return groupSearch ? $"?userid={this.ID}" : $"/{this.ID}";
            }
            else
            {
                return groupSearch ? string.Empty : $"?username={this.Username}";
            }
        }
    }
}
 