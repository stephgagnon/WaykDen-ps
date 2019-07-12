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
        [Parameter(HelpMessage = "List all sessions before given date."), ValidatePattern("\\d{4}-[01]\\d-[0-3]\\dT[0-2]\\d:[0-5]\\d:[0-5]\\d(?:\\.\\d+)?Z?")]
        public string Before {get; set;} = string.Empty;
        [Parameter(HelpMessage = "List all sessions after given date."), ValidatePattern("\\d{4}-[01]\\d-[0-3]\\dT[0-2]\\d:[0-5]\\d:[0-5]\\d(?:\\.\\d+)?Z?")]
        public string After {get; set;} = string.Empty;
        [Parameter(HelpMessage = "List all sessions in progress.")]
        public SwitchParameter InProgress {get; set;} = false;
        [Parameter(HelpMessage = "List all terminated sessions.")]
        public SwitchParameter Terminated {get; set;} = false;

        protected async override void ProcessRecord()
        {
            try
            {
                string parameter = null;
                
                if(!string.IsNullOrEmpty(this.Before) || !string.IsNullOrWhiteSpace(this.After))
                {
                    parameter = this.ParameterBuilder(SessionsGetOptions.ByDate);
                }
                else if(this.InProgress || this.Terminated)
                {
                    parameter = this.ParameterBuilder(SessionsGetOptions.ByState);
                }

                try
                {
                    Task<string> sessionsString = this.DenRestAPIController.GetSessions(parameter);
                    sessionsString.Wait();
                    
                    string res = await sessionsString;

                    if(res.StartsWith('['))
                    {
                        var sessions = this.DenRestAPIController.DeserializeString<Session[]>(res);
                        
                        foreach(Session session in sessions)
                        {
                            this.WriteObject(session.ToSessionObject(), true);
                        }
                    }
                    else
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
            StringBuilder parameter = new StringBuilder("?");
            if(getOption == SessionsGetOptions.ByDate)
            {
                if(!string.IsNullOrEmpty(this.After))
                {
                    DateTime after = this.GetDateTime(this.After);
                    after = after.ToUniversalTime();
                    string rfc = after.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo);
                    parameter.Append($"from={rfc}");
                }

                if(!string.IsNullOrEmpty(this.Before))
                {
                    DateTime before = this.GetDateTime(this.Before);
                    before = before.ToUniversalTime();
                    string rfc = before.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo);
                    parameter.Append($"from={rfc}");
                }
            }
            else
            {
                if(this.InProgress)
                {
                    parameter.Append("state=InProgress");
                } else parameter.Append("state=Terminated");
            }

            return parameter.ToString();
        }

        private DateTime GetDateTime(string value)
        {
            string[] datetime = value.Split('T');
            string[] date = datetime[0]?.Split('-');
            string[] time = datetime[1]?.Split(':');
            return new DateTime
            (
                this.StringToInt(date[0] != null ? date[0]: "00"),
                this.StringToInt(date[1] != null ? date[1]: "00"),
                this.StringToInt(date[2] != null ? date[2]: "00"),
                this.StringToInt(time[0] != null ? time[0]: "00"),
                this.StringToInt(time[1] != null ? time[1]: "00"),
                this.StringToInt(time[2] != null ? time[2]: "00"),
                DateTimeKind.Local
            );
        }

        private int StringToInt(string value)
        {
            bool parsed = Int32.TryParse(value, out int result);
            if(parsed)
            {
                return result;
            }

            return -1;
        }
    }
}