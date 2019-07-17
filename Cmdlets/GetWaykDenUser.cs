using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using WaykDen.Controllers;
using WaykDen.Models;

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

        [Parameter]
        public string Username {get; set;}
        [Parameter(HelpMessage = "User ID.")]
        public string ID {get; set;}
        protected async override void ProcessRecord()
        {
            try
            {
                string parameter = null;
            
                if(!string.IsNullOrEmpty(this.ID))
                {
                    parameter = this.ParameterBuilder(UserGetOption.ByID);
                }
                else if(!string.IsNullOrEmpty(this.Username))
                {
                    parameter = this.ParameterBuilder(UserGetOption.ByUsername);
                }

                Task<string> usersString = this.DenRestAPIController.GetUsers(parameter);
                usersString.Wait();

                string res = await usersString;

                if(res.StartsWith('['))
                {
                    var users = this.DenRestAPIController.DeserializeString<User[]>(res);
                    
                    foreach(User user in users)
                    {
                        this.WriteObject(user.ToUserObject(), true);
                    }
                }
                else if (res.StartsWith('{'))
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

        private string ParameterBuilder(UserGetOption getOption)
        {
            if(getOption == UserGetOption.ByID)
            {
                return $"/{this.ID}";
            }
            else
            {
                return $"?username={this.Username}";
            }
        }
    }
}