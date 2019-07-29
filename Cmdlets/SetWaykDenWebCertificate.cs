using System;
using System.IO;
using System.Management.Automation;
using WaykDen.Controllers;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Set, "WaykDenWebCertificate")]
    public class SetWaykDenWebCertificate : WaykDenConfigCmdlet
    {

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
            try
            {
                DenConfigController denConfigController = new DenConfigController(this.Path, this.Key);
                DenConfig config = denConfigController.GetConfig();

                this.FolderPath = this.FolderPath.EndsWith($"{System.IO.Path.DirectorySeparatorChar}") ? this.FolderPath : $"{this.FolderPath}{System.IO.Path.DirectorySeparatorChar}";
                this.Path = this.Path.EndsWith($"{System.IO.Path.DirectorySeparatorChar}") ? this.Path : $"{this.Path}{System.IO.Path.DirectorySeparatorChar}";

                config.DenTraefikConfigObject.Certificate = File.ReadAllText($"{this.FolderPath}{this.CertificateFile}");
                config.DenTraefikConfigObject.PrivateKey = File.ReadAllText($"{this.FolderPath}{this.PrivateKeyFile}");
                denConfigController.StoreConfig(config);   
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }
    }
}