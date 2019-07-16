using System;
using System.Management.Automation;
using System.Threading.Tasks;
using WaykDen.Controllers;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "WaykDenConnection")]
    public class GetWaykDenConnection : RestApiCmdlet
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

                Task<string> connectionsString = this.DenRestAPIController.GetConnections(parameter);
                connectionsString.Wait();

                string res = await connectionsString;

                if(res.StartsWith('['))
                {
                    var connections = this.DenRestAPIController.DeserializeString<Connection[]>(res);
                    
                    foreach(Connection connection in connections)
                    {
                        this.WriteObject(connection.ToConnectionObject(), true);
                    }
                }
                else if (res.StartsWith('{'))
                {
                    var connection = this.DenRestAPIController.DeserializeString<Connection>(res);
                    this.WriteObject(connection?.ToConnectionObject());
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