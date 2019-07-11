using System;
using System.IO;
using System.Management.Automation;
using WaykPS.Config;

namespace WaykPS.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "WaykDenConfig")]
    public class DenConfig : baseCmdlet
    {
        private const string WAYK_DEN_HOME = "WAYK_DEN_HOME";
        private const string WORKING_DIRECTORY = "Working Directory";
        public string Path {get; set;}
        public DenMongoConfigObject DenMongoConfigObject => this.DenConfigs.DenMongoConfigObject;
        public DenPickyConfigObject DenPickyConfigObject => this.DenConfigs.DenPickyConfigObject;
        public DenLucidConfigObject DenLucidConfigObject => this.DenConfigs.DenLucidConfigObject;
        public DenRouterConfigObject DenRouterConfigObject => this.DenConfigs.DenRouterConfigObject;
        public DenServerConfigObject DenServerConfigObject => this.DenConfigs.DenServerConfigObject;
        public DenTraefikConfigObject DenTraefikConfigObject => this.DenConfigs.DenTraefikConfigObject;
        public DenImageConfigObject DenImageConfigObject => this.DenConfigs.DenImageConfigObject;
        public DenDockerConfigObject DenDockerConfigObject => this.DenConfigs.DenDockerConfigObject;
        public DenConfigs DenConfigs {get; set;}

        public DenConfig()
        {
        }

        protected override void ProcessRecord()
        {
            this.Path = Environment.GetEnvironmentVariable(WAYK_DEN_HOME);
            if(string.IsNullOrEmpty(this.Path))
            {
                this.Path = this.SessionState.Path.CurrentLocation.Path;
            }

            try
            {
                this.Path = this.Path.EndsWith($"{System.IO.Path.DirectorySeparatorChar}") ? $"{this.Path}WaykDen.db" : $"{this.Path}{System.IO.Path.DirectorySeparatorChar}WaykDen.db";
                if(File.Exists(this.Path))
                {
                    DenConfigStore store = new DenConfigStore($"{this.Path}");
                    this.DenConfigs = store.GetConfig();
                }
            }
            catch(Exception e)
            {
                this.WriteWarning("Could not found WaykDen configuration in given path. Make sure WaykDen configuration is in current folder or set WAYK_DEN_HOME to the path of WaykDen configuration");
                this.OnError(e);
            }

            this.WriteObject(new DenObject {Property = $"Mongo : {nameof(this.DenMongoConfigObject.Url)}", Value = this.DenMongoConfigObject.Url});
            this.WriteObject(new DenObject {Property = $"Mongo : {nameof(this.DenMongoConfigObject.Port)}", Value = this.DenMongoConfigObject.Port});
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
            this.WriteObject(new DenObject {Property = $"Traefik : {nameof(this.DenTraefikConfigObject.ApiPort)}", Value = this.DenTraefikConfigObject.ApiPort});
            this.WriteObject(new DenObject {Property = $"Traefik : {nameof(this.DenTraefikConfigObject.WaykDenPort)}", Value = this.DenTraefikConfigObject.WaykDenPort});
            this.WriteObject(new DenObject {Property = $"Image : {nameof(this.DenImageConfigObject.DenMongoImage)}", Value = this.DenImageConfigObject.DenMongoImage});
            this.WriteObject(new DenObject {Property = $"Image : {nameof(this.DenImageConfigObject.DenPickyImage)}", Value = this.DenImageConfigObject.DenPickyImage});
            this.WriteObject(new DenObject {Property = $"Image : {nameof(this.DenImageConfigObject.DenLucidImage)}", Value = this.DenImageConfigObject.DenLucidImage});
            this.WriteObject(new DenObject {Property = $"Image : {nameof(this.DenImageConfigObject.DenRouterImage)}", Value = this.DenImageConfigObject.DenRouterImage});
            this.WriteObject(new DenObject {Property = $"Image : {nameof(this.DenImageConfigObject.DenServerImage)}", Value = this.DenImageConfigObject.DenServerImage});
            this.WriteObject(new DenObject {Property = $"Image : {nameof(this.DenImageConfigObject.DevolutionsJetImage)}", Value = this.DenImageConfigObject.DevolutionsJetImage});
            base.ProcessRecord();
        }
    }
}