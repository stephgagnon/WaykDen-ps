using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Text;
using System.Management.Automation;
using System.Threading.Tasks;
using WaykDen.Controllers;
using WaykDen.Models;
using WaykDen.Utils;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "WaykDenSession")]
    public class GetWaykDenSession : RestApiCmdlet
    {
        private enum SessionsGetOptions
        {
            ByDate,
            ByState
        }
        [Parameter(HelpMessage = "List all sessions before given date.")]
        public DateTime Before {get; set;}
        [Parameter(HelpMessage = "List all sessions after given date.")]
        public DateTime After {get; set;} 
        [Parameter(HelpMessage = "List all terminated sessions.")]
        public SwitchParameter Terminated {get; set;} = false;
        [Parameter(HelpMessage = "List all sessions. (In progress and terminated)")]
        public SwitchParameter All {get; set;} = false;

        protected async override void ProcessRecord()
        {
            try
            {
                string parameter = string.Empty;
                
                if(this.Before != null && this.Before.Year != DateTime.MinValue.Year || this.After != null && this.After.Year != DateTime.MinValue.Year)
                {
                    if(string.IsNullOrEmpty(parameter)) parameter += "?";
                    parameter += this.ParameterBuilder(SessionsGetOptions.ByDate);
                }
                
                if(!this.All)
                {
                    parameter += string.IsNullOrEmpty(parameter) ? "?" : "&";
                    parameter += this.ParameterBuilder(SessionsGetOptions.ByState);
                }

                try
                {
                    Task<string> sessionsString = this.DenRestAPIController.GetSessions(parameter);
                    sessionsString.Wait();
                    
                    string res = await sessionsString;

                    if(res.StartsWith("["))
                    {
                        var sessions = this.DenRestAPIController.DeserializeString<Session[]>(res);
                        
                        foreach(Session session in sessions)
                        {
                            this.WriteObject(session.ToSessionObject(), true);
                        }
                    }
                    else if (res.StartsWith("{"))
                    {
                        var session = this.DenRestAPIController.DeserializeString<Session>(res);
                        this.WriteObject(session?.ToSessionObject());
                    }
                }
                catch(Exception e)
                {
                    this.OnError(e);
                }
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }

        private string ParameterBuilder(SessionsGetOptions getOption)
        {
            StringBuilder parameter = new StringBuilder();
            if(getOption == SessionsGetOptions.ByDate)
            {
                if(this.After != null && this.After.Year != DateTime.MinValue.Year)
                {
                    string rfc = this.After.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo);
                    parameter.Append($"from={rfc}");

                    if(this.Before != null)
                    {
                        parameter.Append("&");
                    }
                }

                if(this.Before != null && this.Before.Year != DateTime.MinValue.Year)
                {
                    string rfc = this.Before.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo);
                    parameter.Append($"to={rfc}");
                }
            }
            else
            {
                if(this.Terminated)
                {
                    parameter.Append("state=Terminated");
                } 
                else parameter.Append("state=InProgress");
            }

            return parameter.ToString();
        }
    }
}