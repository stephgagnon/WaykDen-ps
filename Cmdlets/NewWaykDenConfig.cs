using System;
using System.IO;
using System.Management.Automation;
using WaykDen.Controllers;
using WaykDen.Models;
using WaykDen.Utils;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "WaykDenConfig")]
    public class NewWaykDenConfig : WaykDenConfigCmdlet
    {
        public const string DOCKER_DEFAULT_CLIENT_URI_LINUX = "unix:///var/run/docker.sock";
        public const string DOCKER_DEFAULT_CLIENT_URI_WINDOWS = "npipe://./pipe/docker_engine";
        private DenConfig DenConfig {get; set;}
        [Parameter(HelpMessage = "Url of a running MongoDB instance.")]
        public string MongoUrl {get; set;} = string.Empty;
        [Parameter(HelpMessage = "Port of a runnning MongoDB instance.")]
        public string MongoPort {get; set;} = string.Empty;
        [Parameter(Mandatory=true, HelpMessage = "Name of domain for WaykDen. (Not a DNS domain)")]
        public string Realm {get; set;} = string.Empty;
        [Parameter(Mandatory=true, HelpMessage = "WaykDen server external URL.")]
        public string ExternalUrl {get; set;} = string.Empty;
        [Parameter(HelpMessage = "LDAP or AD server URL")]
        public string LDAPServerUrl {get; set;} = string.Empty;
        [Parameter(HelpMessage = "Specify a username with read access on AD.")]
        public string LDAPUsername {get; set;} = string.Empty;
        [Parameter(HelpMessage = "Password of the user with read access on AD.")]
        public string LDAPPassword {get; set;} = string.Empty;
        [Parameter(HelpMessage = " Group of users who can be authenticated on WaykDen.")]
        public string LDAPUserGroup {get; set;} = string.Empty;
        [Parameter(HelpMessage = "Type of LDAP server. ActiveDirectory to use AD integration; JumpCloud to use JumpCloud integration"),
         ValidateSet(new string[]{"ActiveDirectory", "JumpCloud"})]
        public string LDAPServerType {get; set;} = string.Empty;
        [Parameter(HelpMessage = "Base DN is the Distinguished Named (DN) where all users and groups can be found. Example: Exemple : ou=Users,o=YOUR_ORG_ID,dc=jumpcloud,dc=com")]
        public string LDAPBaseDN {get; set;} = string.Empty;
        [Parameter(HelpMessage = "URL where Devolutions Jet will be listening.")]
        public string JetServerUrl {get; set;} = string.Empty;
        [Parameter(HelpMessage = "Force the Wayk client to be logged and authenticated. WaykDen will give an ID only if the user is authenticated.")]
        public SwitchParameter LoginRequired {get; set;} = false;
        [Parameter(HelpMessage = "Docker client endpoint URI.")]
        public string DockerClientUri {get; set;} = string.Empty;
        [Parameter(HelpMessage = "Port where traefik API will be listening.")]
        public string TraefikApiPort {get; set;} = "8080";
        [Parameter(HelpMessage = "Port where WaykDen server will be listening.")]
        public string WaykDenPort {get; set;} = "4000";
        [Parameter(HelpMessage = "Path to a x509 certificate to use https with Traefik.")]
        public string CertificatePath {get; set;} = string.Empty;
        [Parameter(HelpMessage = "Path to the private key of the given certificate for Traefik.")]
        public string PrivateKeyPath {get; set;} = string.Empty;
        [Parameter(HelpMessage = "Use Linux or Windows  container."), ValidateSet(new string[]{"Linux", "Windows"})]
        public string Platform {get; set;} = string.Empty;
        [Parameter(HelpMessage = "URL of a syslog server.")]
        public string SyslogServer {get; set;} = string.Empty;
        private string dockerDefaultEndpoint
        {
            get
            {
                if(Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    return DOCKER_DEFAULT_CLIENT_URI_LINUX;
                } 
                else 
                {
                    return DOCKER_DEFAULT_CLIENT_URI_WINDOWS;
                }
            }
        }

        public NewWaykDenConfig()
        {
        }

        protected override void ProcessRecord()
        {
            try
            {
                DenConfigController denConfigController = new DenConfigController(this.Path, this.Key);

                if(denConfigController.DbExists)
                {
                    this.DenConfig = denConfigController.GetConfig();
                    this.UpdateConfig();
                }
                else
                {
                    if(string.IsNullOrEmpty(this.Platform))
                    {
                        this.Platform = "Linux";
                    }

                    Platforms platform = this.Platform.Equals("Linux") ? Platforms.Linux : Platforms.Windows;
                    RsaKeyGenerator rsaKeyGenerator = new RsaKeyGenerator();
                    this.DenConfig = new DenConfig()
                    {
                        DenDockerConfigObject = new DenDockerConfigObject
                        {
                            DockerClientUri = string.IsNullOrEmpty(this.DockerClientUri) ? this.dockerDefaultEndpoint : this.DockerClientUri,
                            Platform = platform.ToString(),
                            SyslogServer = this.SyslogServer
                        },

                        DenMongoConfigObject = new DenMongoConfigObject
                        {
                            Port = this.MongoPort != null ? this.MongoPort : string.Empty,
                            Url = this.MongoUrl != null ? this.MongoUrl : string.Empty
                        },

                        DenPickyConfigObject = new DenPickyConfigObject
                        {
                            ApiKey = DenServiceUtils.Generate(32),
                            Backend = "mongodb",
                            Realm = this.Realm
                        },

                        DenLucidConfigObject = new DenLucidConfigObject
                        {
                            AdminSecret = DenServiceUtils.Generate(10),
                            AdminUsername =  DenServiceUtils.Generate(16),
                            ApiKey = DenServiceUtils.Generate(32)
                        },

                        DenRouterConfigObject = new DenRouterConfigObject
                        {
                            PublicKey = RsaKeyutils.PemToDer(rsaKeyGenerator.PublicKey)
                        },

                        DenServerConfigObject = new DenServerConfigObject
                        {
                            ApiKey = DenServiceUtils.Generate(32),
                            AuditTrails = "true",
                            ExternalUrl = this.ExternalUrl,
                            LDAPPassword = this.LDAPPassword != null ? this.LDAPPassword : string.Empty,
                            LDAPServerUrl = this.LDAPServerUrl != null ? this.LDAPServerUrl : string.Empty,
                            LDAPUserGroup = this.LDAPUserGroup != null ? this.LDAPUserGroup : string.Empty,
                            LDAPUsername = this.LDAPUsername != null ? this.LDAPUsername : string.Empty,
                            LDAPServerType = this.LDAPServerType != null ? this.LDAPServerType : string.Empty,
                            LDAPBaseDN = this.LDAPBaseDN != null ? this.LDAPBaseDN : string.Empty,
                            PrivateKey = RsaKeyutils.PemToDer(rsaKeyGenerator.PrivateKey),
                            JetServerUrl = this.JetServerUrl != null ? this.JetServerUrl : string.Empty,
                            LoginRequired = this.LoginRequired ? "True": "False"
                        },

                        DenTraefikConfigObject = new DenTraefikConfigObject
                        {
                            ApiPort = this.TraefikApiPort != null ? this.TraefikApiPort : "8080",
                            WaykDenPort = this.WaykDenPort != null ? this.WaykDenPort : "4000",
                            Certificate = this.CertificatePath != null ? this.CertificatePath : string.Empty,
                            PrivateKey = this.PrivateKeyPath != null ? this.PrivateKeyPath : string.Empty
                        },

                        DenImageConfigObject = new DenImageConfigObject(platform)
                    };
                }

                denConfigController.StoreConfig(this.DenConfig);
                Environment.SetEnvironmentVariable(WAYK_DEN_HOME, this.Path);
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
            
            base.ProcessRecord();
        }

        private void UpdateConfig()
        {
            this.DenConfig.DenDockerConfigObject.DockerClientUri = !string.IsNullOrEmpty(this.DockerClientUri) ? this.DockerClientUri : this.DenConfig.DenDockerConfigObject.DockerClientUri;
            this.DenConfig.DenDockerConfigObject.SyslogServer = !string.IsNullOrEmpty(this.SyslogServer) ? this.SyslogServer : this.DenConfig.DenDockerConfigObject.SyslogServer;
            this.DenConfig.DenMongoConfigObject.Port = !string.IsNullOrEmpty(this.MongoPort) ? this.MongoPort : this.DenConfig.DenMongoConfigObject.Port;
            this.DenConfig.DenMongoConfigObject.Url = !string.IsNullOrEmpty(this.MongoUrl) ? this.MongoUrl : this.DenConfig.DenMongoConfigObject.Url;
            this.DenConfig.DenPickyConfigObject.Realm = !string.IsNullOrEmpty(this.Realm) ? this.Realm: this.DenConfig.DenPickyConfigObject.Realm;
            this.DenConfig.DenServerConfigObject.ExternalUrl = !string.IsNullOrEmpty(this.ExternalUrl) ? this.ExternalUrl : this.DenConfig.DenServerConfigObject.ExternalUrl;
            this.DenConfig.DenServerConfigObject.LDAPServerType = !string.IsNullOrEmpty(this.LDAPServerType) ? this.LDAPServerType : this.DenConfig.DenServerConfigObject.LDAPServerType;
            this.DenConfig.DenServerConfigObject.LDAPBaseDN = !string.IsNullOrEmpty(this.LDAPBaseDN) ? this.LDAPBaseDN : this.DenConfig.DenServerConfigObject.LDAPBaseDN;
            this.DenConfig.DenServerConfigObject.LDAPUsername = !string.IsNullOrEmpty(this.LDAPUsername) ? this.LDAPUsername : this.DenConfig.DenServerConfigObject.LDAPUsername;
            this.DenConfig.DenServerConfigObject.LDAPPassword = !string.IsNullOrEmpty(this.LDAPPassword) ? this.LDAPPassword : this.DenConfig.DenServerConfigObject.LDAPPassword;
            this.DenConfig.DenServerConfigObject.LDAPUserGroup = !string.IsNullOrEmpty(this.LDAPUserGroup) ? this.LDAPUserGroup : this.DenConfig.DenServerConfigObject.LDAPUserGroup;
            this.DenConfig.DenServerConfigObject.JetServerUrl = !string.IsNullOrEmpty(this.JetServerUrl) ? this.JetServerUrl : this.DenConfig.DenServerConfigObject.JetServerUrl;
            this.DenConfig.DenServerConfigObject.LoginRequired = this.LoginRequired ? "True" : "False";
            this.DenConfig.DenServerConfigObject.LDAPServerUrl = !string.IsNullOrEmpty(this.LDAPServerUrl) ? this.LDAPServerUrl : this.DenConfig.DenServerConfigObject.LDAPServerUrl;
            this.DenConfig.DenTraefikConfigObject.ApiPort = !string.IsNullOrEmpty(this.TraefikApiPort) ? this.TraefikApiPort : this.DenConfig.DenTraefikConfigObject.ApiPort;
            this.DenConfig.DenTraefikConfigObject.WaykDenPort = !string.IsNullOrEmpty(this.WaykDenPort) ? this.WaykDenPort : this.DenConfig.DenTraefikConfigObject.WaykDenPort;

            if(!string.IsNullOrEmpty(this.Platform) && !this.Platform.Equals(this.DenConfig.DenDockerConfigObject.Platform))
            {
                this.DenConfig.DenDockerConfigObject.Platform = this.Platform;
                Platforms platform = this.Platform.Equals("Linux") ? Platforms.Linux : Platforms.Windows;
                this.DenConfig.DenImageConfigObject = new DenImageConfigObject(platform);
            }
        }
    }
}