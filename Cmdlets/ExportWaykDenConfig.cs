using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
{
    [Cmdlet("Export", "WaykDenConfig")]
    public class ExportWaykDenConfig : WaykDenConfigCmdlet
    {
        private const string FILENAME = "docker-compose.yml";
        [Parameter(HelpMessage = "Path where to export WaykDen configuration.")]
        public string ExportPath {get; set;} = string.Empty;
        public ExportWaykDenConfig()
        {
        }

        protected override void ProcessRecord()
        {
            try
            {
                DenServicesController denServicesController = new DenServicesController(this.Path, this.Key);
                denServicesController.OnLog += this.OnLog;
                denServicesController.OnError += this.OnError;

                if(string.IsNullOrEmpty(this.ExportPath))
                {
                    this.ExportPath = this.SessionState.Path.CurrentLocation.Path;
                }

                this.ExportPath.TrimEnd(System.IO.Path.DirectorySeparatorChar);
                File.WriteAllText($"{this.ExportPath}{System.IO.Path.DirectorySeparatorChar}{FILENAME}", denServicesController.CreateDockerCompose());
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }
    }
}