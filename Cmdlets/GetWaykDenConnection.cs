using System;
using System.Management.Automation;
using System.Threading.Tasks;
using WaykPS.Controllers;
using WaykPS.RestAPI;

namespace WaykPS.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "WaykDenConnection")]
    public class GetWaykDenConnection : baseCmdlet
    {
        private enum ConnectionGetOptions
        {
            ByID,
            ByState
        }
        [Parameter(HelpMessage = "A Connection ID.")]
        public string ID {get; set;} = string.Empty;
        [Parameter(HelpMessage = "List all offline connections")]
        public SwitchParameter Offline = false;
        public GetWaykDenConnection()
        {
        }

        protected async override void ProcessRecord()
        {
            try
            {
                string parameter = null;

                if(!string.IsNullOrEmpty(this.ID))
                {
                    parameter = this.ParameterBuilder(ConnectionGetOptions.ByID);
                }
                else
                {
                    parameter = this.ParameterBuilder(ConnectionGetOptions.ByState);
                }

                DenRestAPIController denRestAPIController = new DenRestAPIController(this.SessionState.Path.CurrentLocation.Path);
                denRestAPIController.OnError += this.OnError;

                Task<string> connectionsString = denRestAPIController.GetConnections(parameter);
                connectionsString.Wait();

                string res = await connectionsString;

                if(string.IsNullOrEmpty(this.ID))
                {
                    var connections = denRestAPIController.DeserializeString<Connection[]>(res);

                    foreach(Connection connection in connections)
                    {
                        this.WriteObject(connection.ToConnectionObject(), true);
                    }
                }
                else 
                {
                    var connection  = denRestAPIController.DeserializeString<Connection>(res);
                    this.WriteObject(connection?.ToConnectionObject(), true);
                }
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }

        private string ParameterBuilder(ConnectionGetOptions getOption)
        {
            if(getOption == ConnectionGetOptions.ByID)
            {
                return $"/{this.ID}";
            }
            else
            {
                if(this.Offline)
                {
                    return "?state=Offline";
                } else return "?state=Online";
            }
        }
    }
}