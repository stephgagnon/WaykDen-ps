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
        private const string WAYK_DEN_HOME = "WAYK_DEN_HOME";
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
        [Parameter]
        public string MongoImage {get; set;} = "library/mongo:4.1-bionic";
        [Parameter]
        public string DenLucidImage {get; set;} = "devolutions/den-lucid:3.3.3-stretch-dev";
        [Parameter]
        public string PickyImage {get; set;} = "devolutions/picky:3.0.0-stretch-dev";
        [Parameter]
        public string DenRouterImage {get; set;} = "devolutions/den-router:0.5.0-stretch-dev";
        [Parameter]
        public string DenServerImage {get; set;} = "devolutions/den-server:1.2.0-stretch-dev";
        [Parameter]
        public string DenTraefikImage {get; set;} = "library/traefik:1.7";
        [Parameter]
        public string DevolutionsJetImage {get; set;} = "devolutions/devolutions-jet:0.4.0-stretch";

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
                    RsaKeyGenerator rsaKeyGenerator = new RsaKeyGenerator();
                    this.DenConfig = new DenConfig()
                    {
                        DenDockerConfigObject = new DenDockerConfigObject
                        {
                            DockerClientUri = this.DockerClientUri != null ? this.DockerClientUri : string.Empty
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
                            JetServerUrl = this.JetServerUrl != null ? this.JetServerUrl : string.Empty
                        },

                        DenTraefikConfigObject = new DenTraefikConfigObject
                        {
                            ApiPort = this.TraefikApiPort != null ? this.TraefikApiPort : "8080",
                            WaykDenPort = this.WaykDenPort != null ? this.WaykDenPort : "4000",
                            Certificate = this.CertificatePath != null ? this.CertificatePath : string.Empty,
                            PrivateKey = this.PrivateKeyPath != null ? this.PrivateKeyPath : string.Empty
                        },

                        DenImageConfigObject = new DenImageConfigObject
                        {   
                            DenMongoImage = this.MongoImage != null ? this.MongoImage : "library/mongo",
                            DenLucidImage = this.DenLucidImage != null ? this.DenLucidImage : "devolutions/den-lucid:3.3.3-stretch-dev",
                            DenPickyImage = this.PickyImage != null ? this.PickyImage : "devolutions/picky:3.0.0-stretch-dev",
                            DenRouterImage = this.DenRouterImage != null ? this.DenRouterImage : "devolutions/den-router:0.5.0-stretch-dev",
                            DenServerImage = this.DenServerImage != null ? this.DenServerImage : "devolutions/den-server:1.2.0-stretch-dev",
                            DenTraefikImage = this.DenTraefikImage != null ? this.DenTraefikImage : "library/traefik:1.7",
                            DevolutionsJetImage = this.DevolutionsJetImage != null ? this.DevolutionsJetImage : "devolutions/devolutions-jet:0.4.0-stretch",
                        }
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
            this.DenConfig.DenTraefikConfigObject.ApiPort = !string.IsNullOrEmpty(this.TraefikApiPort) ? this.TraefikApiPort : this.DenConfig.DenTraefikConfigObject.ApiPort;
            this.DenConfig.DenTraefikConfigObject.WaykDenPort = !string.IsNullOrEmpty(this.WaykDenPort) ? this.WaykDenPort : this.DenConfig.DenTraefikConfigObject.WaykDenPort;
        }
    }
}