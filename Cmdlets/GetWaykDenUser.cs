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
    public class GetWaykDenUser : baseCmdlet
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
        
        public GetWaykDenUser()
        {
        }

        protected async override void ProcessRecord()
        {
            DenRestAPIController denRestAPIController = new DenRestAPIController(this.SessionState.Path.CurrentLocation.Path);
            denRestAPIController.OnError += this.OnError;
            string parameter = null;
            
            if(!string.IsNullOrEmpty(this.ID))
            {
                parameter = this.ParameterBuilder(UserGetOption.ByID);
            }
            else if(!string.IsNullOrEmpty(this.Username))
            {
                parameter = this.ParameterBuilder(UserGetOption.ByUsername);
            }

            Task<string> usersString = denRestAPIController.GetUsers(parameter);
            usersString.Wait();

            string res = await usersString;

            if(res.StartsWith('['))
            {
                var users = denRestAPIController.DeserializeString<User[]>(res);
                
                foreach(User user in users)
                {
                    this.WriteObject(user.ToUserObject(), true);
                }
            }
            else
            {
                var user = denRestAPIController.DeserializeString<User>(res);
                this.WriteObject(user?.ToUserObject());
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