using System;
using System.IO;
using System.Management.Automation;
using WaykPS.Controllers;
using WaykPS.Config;

namespace WaykPS.Cmdlets
{
    [Cmdlet(VerbsCommon.Set, "WaykDenWebCertificate")]
    public class SetWaykDenWebCertificate : baseCmdlet
    {
        private const string WAYK_DEN_HOME = "WAYK_DEN_HOME";
        private DenRestAPIController denRestAPIController {get; set;}
        private string Path {get; set;} = string.Empty;

        [Parameter(Mandatory = true, HelpMessage = "Path to a directory where a certificate and its private key are found.")]
        public string FolderPath {get; set;} = string.Empty;
        [Parameter(Mandatory = true, HelpMessage = "Name of a certificate with its extension.")]
        public string CertificateFile {get; set;} = string.Empty;
        [Parameter(Mandatory = true, HelpMessage = "Name of a private key with its extension.")]
        public string PrivateKeyFile {get; set;} = string.Empty;

        public SetWaykDenWebCertificate()
        {
        }

        protected override void ProcessRecord()
        {
            this.Path = Environment.GetEnvironmentVariable(WAYK_DEN_HOME);
            if(string.IsNullOrEmpty(this.Path))
            {
                this.Path = this.SessionState.Path.CurrentLocation.Path;
            }

            DenConfigStore store = new DenConfigStore($"{this.Path}/WaykDen.db");
            DenConfigs configs = store.GetConfig();

            this.FolderPath = this.FolderPath.EndsWith($"{System.IO.Path.DirectorySeparatorChar}") ? this.FolderPath : $"{this.FolderPath}{System.IO.Path.DirectorySeparatorChar}";
            this.Path = this.Path.EndsWith($"{System.IO.Path.DirectorySeparatorChar}") ? this.Path : $"{this.Path}{System.IO.Path.DirectorySeparatorChar}";

            configs.DenTraefikConfigObject.Certificate = File.ReadAllText($"{this.FolderPath}{this.CertificateFile}");
            configs.DenTraefikConfigObject.PrivateKey = File.ReadAllText($"{this.FolderPath}{this.PrivateKeyFile}");
            store.StoreConfig(new DenConfig{DenConfigs = configs});
        }
    }
}