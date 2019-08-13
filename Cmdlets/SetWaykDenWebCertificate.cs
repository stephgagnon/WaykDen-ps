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
        [Parameter(Mandatory = true, HelpMessage = "Path to a x509 certificate or chain in PEM format.")]
        public string CertificatePath {get; set;} = string.Empty;
        [Parameter(Mandatory = true, HelpMessage = "Path to the private key of the given certificate for Traefik. In a case of a chain, the leaf certicate private key")]
        public string PrivateKeyPath {get; set;} = string.Empty;

        public SetWaykDenWebCertificate()
        {
        }

        protected override void ProcessRecord()
        {
            try
            {
                DenConfigController denConfigController = new DenConfigController(this.Path, this.Key);
                DenConfig config = denConfigController.GetConfig();

                config.DenTraefikConfigObject.Certificate = File.ReadAllText(CertificatePath);
                config.DenTraefikConfigObject.PrivateKey = File.ReadAllText(PrivateKeyPath);
                denConfigController.StoreConfig(config);
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }
    }
}