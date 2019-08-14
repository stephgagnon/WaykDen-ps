using System;
using System.IO;
using System.Management.Automation;
using WaykDen.Models;
using WaykDen.Controllers;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "WaykDenConfig")]
    public class GetWaykDenConfig : WaykDenConfigCmdlet
    {
        private const string WORKING_DIRECTORY = "Working Directory";
        public DenMongoConfigObject DenMongoConfigObject => this.DenConfig.DenMongoConfigObject;
        public DenPickyConfigObject DenPickyConfigObject => this.DenConfig.DenPickyConfigObject;
        public DenLucidConfigObject DenLucidConfigObject => this.DenConfig.DenLucidConfigObject;
        public DenRouterConfigObject DenRouterConfigObject => this.DenConfig.DenRouterConfigObject;
        public DenServerConfigObject DenServerConfigObject => this.DenConfig.DenServerConfigObject;
        public DenTraefikConfigObject DenTraefikConfigObject => this.DenConfig.DenTraefikConfigObject;
        public DenImageConfigObject DenImageConfigObject => this.DenConfig.DenImageConfigObject;
        public DenDockerConfigObject DenDockerConfigObject => this.DenConfig.DenDockerConfigObject;
        public DenConfig DenConfig {get; set;}

        protected override void ProcessRecord()
        {
            try
            {
                this.DenConfigController = new DenConfigController(this.Path, this.Key);
                if(this.DenConfigController.DbExists)
                {
                    this.DenConfig = this.DenConfigController.GetConfig();
                }
                else
                {
                    throw new Exception("Could not found WaykDen configuration in given path. Use New-WaykDenConfig or make sure WaykDen configuration is in current folder or set WAYK_DEN_HOME to the path of WaykDen configuration");
                }
            }
            catch(Exception e)
            {
                this.OnError(e);
            }

            this.WriteObject(new DenObject {Property = "Docker client uri", Value = this.DenDockerConfigObject.DockerClientUri});
            this.WriteObject(new DenObject {Property = nameof(this.DenDockerConfigObject.Platform), Value = this.DenDockerConfigObject.Platform});
            this.WriteObject(new DenObject {Property = $"{nameof(this.DenDockerConfigObject.SyslogServer)}", Value = this.DenDockerConfigObject.SyslogServer});
            this.WriteObject(new DenObject {Property = $"Mongo : {nameof(this.DenMongoConfigObject.Url)}", Value = this.DenMongoConfigObject.Url});
            this.WriteObject(new DenObject {Property = $"Den-Picky : {nameof(this.DenPickyConfigObject.ApiKey)}", Value = this.DenPickyConfigObject.ApiKey});
            this.WriteObject(new DenObject {Property = $"Den-Picky : {nameof(this.DenPickyConfigObject.Realm)}", Value = this.DenPickyConfigObject.Realm});
            this.WriteObject(new DenObject {Property = $"Den-Picky : {nameof(this.DenPickyConfigObject.Backend)}", Value = this.DenPickyConfigObject.Backend});
            this.WriteObject(new DenObject {Property = $"Den-Lucid : {nameof(this.DenLucidConfigObject.ApiKey)}", Value = this.DenLucidConfigObject.ApiKey});
            this.WriteObject(new DenObject {Property = $"Den-Lucid : {nameof(this.DenLucidConfigObject.AdminSecret)}", Value = this.DenLucidConfigObject.AdminSecret});
            this.WriteObject(new DenObject {Property = $"Den-Lucid : {nameof(this.DenLucidConfigObject.AdminUsername)}", Value = this.DenLucidConfigObject.AdminUsername});
            this.WriteObject(new DenObject {Property = $"Den-Server : {nameof(this.DenServerConfigObject.ExternalUrl)}", Value = this.DenServerConfigObject.ExternalUrl});
            this.WriteObject(new DenObject {Property = $"Den-Server : {nameof(this.DenServerConfigObject.LDAPServerType)}", Value = this.DenServerConfigObject.LDAPServerType});
            this.WriteObject(new DenObject {Property = $"Den-Server : {nameof(this.DenServerConfigObject.LDAPUsername)}", Value = this.DenServerConfigObject.LDAPUsername});
            this.WriteObject(new DenObject {Property = $"Den-Server : {nameof(this.DenServerConfigObject.LDAPPassword)}", Value = this.DenServerConfigObject.LDAPPassword});
            this.WriteObject(new DenObject {Property = $"Den-Server : {nameof(this.DenServerConfigObject.LDAPServerUrl)}", Value = this.DenServerConfigObject.LDAPServerUrl});
            this.WriteObject(new DenObject {Property = $"Den-Server : {nameof(this.DenServerConfigObject.LDAPUserGroup)}", Value = this.DenServerConfigObject.LDAPUserGroup});
            this.WriteObject(new DenObject {Property = $"Den-Server : {nameof(this.DenServerConfigObject.LDAPBaseDN)}", Value = this.DenServerConfigObject.LDAPBaseDN});
            this.WriteObject(new DenObject {Property = $"Den-Server : {nameof(this.DenServerConfigObject.ApiKey)}", Value = this.DenServerConfigObject.ApiKey});
            this.WriteObject(new DenObject {Property = $"Den-Server : {nameof(this.DenServerConfigObject.JetServerUrl)}", Value = this.DenServerConfigObject.JetServerUrl});
            this.WriteObject(new DenObject {Property = $"Den-Server : {nameof(this.DenServerConfigObject.LoginRequired)}", Value = this.DenServerConfigObject.LoginRequired});
            this.WriteObject(new DenObject {Property = $"Traefik : {nameof(this.DenTraefikConfigObject.WaykDenPort)}", Value = this.DenTraefikConfigObject.WaykDenPort});
            base.ProcessRecord();
        }
    }
}