using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using WaykDen.Controllers;
using WaykDen.Models;

namespace WaykDen.Cmdlets
{
    [Cmdlet(VerbsCommon.Set, "WaykDenConfig")]
    public class SetWaykDenConfig : WaykDenConfigCmdlet
    {
        [Parameter(HelpMessage = "Url of a running MongoDB instance.")]
        public string MongoUrl {get; set;}= string.Empty;
        [Parameter(HelpMessage = "Port of a running MongoDB instance.")]
        public string MongoPort {get; set;}= string.Empty;
        [Parameter(HelpMessage = "Name of a domain for WaykDen. (Not a DNS domain)")]
        public string Realm {get; set;}= string.Empty;
        [Parameter(HelpMessage = "WaykDen server external URL.")]
        public string ExternalUrl {get; set;}= string.Empty;
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
        public string JetServerUrl {get; set;} = string.Empty;
        [Parameter(HelpMessage = "Docker client endpoint URI.")]
        public string DockerClientUri {get; set;}= string.Empty;
        [Parameter(HelpMessage = "Port where traefik API will be listening.")]
        public string TraefikApiPort {get; set;}= string.Empty;
        [Parameter(HelpMessage = "Port where WaykDen server will be listening.")]
        public string WaykDenPort {get; set;}= string.Empty;
        [Parameter(HelpMessage = "Path to a x509 certificate to use https with Traefik.")]
        public string CertificatePath {get; set;} = string.Empty;
        [Parameter(HelpMessage = "Path to the private key of the given certificate for Traefik.")]
        public string KeyPath {get; set;} = string.Empty;
        [Parameter(HelpMessage = "Force the Wayk client to be logged and authenticated. WaykDen will give an ID only if the user is authenticated."), ValidateSet(new string[]{"True", "False"})]
        public string LoginRequired {get; set;} = string.Empty;
        [Parameter(HelpMessage = "Use Linux or Windows  container."), ValidateSet(new string[]{"Linux", "Windows"})]
        public string Platform {get; set;} = "Linux";
        [Parameter(HelpMessage = "URL of a syslog server.")]
        public string SyslogServer {get; set;} = string.Empty;
        [Parameter(HelpMessage = "Remove parameter"), ValidateSet(
            new string[]
            {
                "MongoUrl",
                "MongoPort",
                "Realm",
                "ExternalUrl",
                "LDAPServerUrl",
                "LDAPUsername",
                "LDAPPassword",
                "LDAPUserGroup",
                "LDAPServerType",
                "LDAPBaseDN",
                "DockerClientUri",
                "TraefikApiPort",
                "WaykDenPort",
                "CertificatePath",
                "KeyPath",
                "SyslogServer",
            }
        )]
        public string[] Remove {get; set;} = null;
        public string MongoImage {get; set;} = string.Empty;
        [Parameter]
        public string PickyImage {get; set;} = string.Empty;
        [Parameter]
        public string DenLucidImage {get; set;} = string.Empty;
        [Parameter]
        public string DenRouterImage {get; set;} = string.Empty;
        [Parameter]
        public string DenServerImage {get; set;} = string.Empty;
        [Parameter]
        public string TraefikImage {get; set;} = string.Empty;
        [Parameter]
        public string DevolutionsJetImage {get; set;} = string.Empty;
        private Dictionary<string, (Type, string)> dictionary;
        public SetWaykDenConfig()
        {
            this.dictionary = new Dictionary<string, (Type, string)>
            {
                {nameof(this.DockerClientUri), (typeof(DenDockerConfigObject), "DockerClientUri")},
                {nameof(this.Platform), (typeof(DenDockerConfigObject), "Platform")},
                {nameof(this.SyslogServer), (typeof(DenDockerConfigObject), "SyslogServer")},
                {nameof(this.MongoUrl), (typeof(DenMongoConfigObject), "Url")},
                {nameof(this.MongoPort), (typeof(DenMongoConfigObject), "Port")},
                {nameof(this.Realm), (typeof(DenPickyConfigObject), "Realm")},
                {nameof(this.ExternalUrl), (typeof(DenServerConfigObject), "ExternalUrl")},
                {nameof(this.LDAPPassword), (typeof(DenServerConfigObject), "LDAPPassword")},
                {nameof(this.LDAPUsername), (typeof(DenServerConfigObject), "LDAPUsername")},
                {nameof(this.LDAPServerUrl), (typeof(DenServerConfigObject), "LDAPServerUrl")},
                {nameof(this.LDAPUserGroup), (typeof(DenServerConfigObject), "LDAPUserGroup")},
                {nameof(this.LDAPServerType), (typeof(DenServerConfigObject), "LDAPServerType")},
                {nameof(this.LDAPBaseDN), (typeof(DenServerConfigObject), "LDAPBaseDN")},
                {nameof(this.JetServerUrl), (typeof(DenServerConfigObject), "JetServerUrl")},
                {nameof(this.LoginRequired), (typeof(DenServerConfigObject), "LoginRequired")},
                {nameof(this.TraefikApiPort), (typeof(DenTraefikConfigObject), "ApiPort")},
                {nameof(this.WaykDenPort), (typeof(DenTraefikConfigObject), "WaykDenPort")},
                {nameof(this.CertificatePath), (typeof(DenTraefikConfigObject), "CertificatePath")},
                {nameof(this.KeyPath), (typeof(DenTraefikConfigObject), "KeyPath")},
                {nameof(this.MongoImage), (typeof(DenImageConfigObject), "DenMongoImage")},
                {nameof(this.PickyImage), (typeof(DenImageConfigObject), "DenPickyImage")},
                {nameof(this.DenLucidImage), (typeof(DenImageConfigObject), "DenLucidImage")},
                {nameof(this.DenRouterImage), (typeof(DenImageConfigObject), "DenRouterImage")},
                {nameof(this.DenServerImage), (typeof(DenImageConfigObject), "DenServerImage")},
                {nameof(this.TraefikImage), (typeof(DenImageConfigObject), "DenTraefikImage")},
                {nameof(this.DevolutionsJetImage), (typeof(DenImageConfigObject), "DevolutionsJetImage")}
            };
        }

        protected override void ProcessRecord()
        {
            try
            { 
                DenImageConfigObject denImages = null;
                if(!string.IsNullOrEmpty(this.Platform))
                {
                    Platforms platform = this.Platform.Equals("Linux") ? Platforms.Linux : Platforms.Windows;
                    denImages = new DenImageConfigObject(platform);
                    this.MongoImage = denImages.DenMongoImage;
                    this.PickyImage = denImages.DenPickyImage;
                    this.DenLucidImage = denImages.DenLucidImage;
                    this.DenRouterImage = denImages.DenRouterImage;
                    this.DenServerImage = denImages.DenServerImage;
                    this.TraefikImage = denImages.DenTraefikImage;
                    this.DevolutionsJetImage = denImages.DevolutionsJetImage;
                }


                (string, bool)[] values = new (string, bool)[]
                {
                    (nameof(this.DockerClientUri), !string.IsNullOrEmpty(this.DockerClientUri)),
                    (nameof(this.Platform), !string.IsNullOrEmpty(this.Platform)),
                    (nameof(this.SyslogServer), !string.IsNullOrEmpty(this.SyslogServer)),
                    (nameof(this.MongoUrl), !string.IsNullOrEmpty(this.MongoUrl)),
                    (nameof(this.MongoPort), !string.IsNullOrEmpty(this.MongoPort)),
                    (nameof(this.Realm), !string.IsNullOrEmpty(this.Realm)),
                    (nameof(this.ExternalUrl), !string.IsNullOrEmpty(this.ExternalUrl)),
                    (nameof(this.LDAPPassword), !string.IsNullOrEmpty(this.LDAPPassword)),
                    (nameof(this.LDAPServerUrl), !string.IsNullOrEmpty(this.LDAPServerUrl)),
                    (nameof(this.LDAPUserGroup), !string.IsNullOrEmpty(this.LDAPUserGroup)),
                    (nameof(this.LDAPUsername), !string.IsNullOrEmpty(this.LDAPUsername)),
                    (nameof(this.LDAPServerType), !string.IsNullOrEmpty(this.LDAPServerType)),
                    (nameof(this.LDAPBaseDN), !string.IsNullOrEmpty(this.LDAPBaseDN)),
                    (nameof(this.JetServerUrl), !string.IsNullOrEmpty(this.JetServerUrl)),
                    (nameof(this.LoginRequired), !string.IsNullOrEmpty(this.LoginRequired)),
                    (nameof(this.TraefikApiPort), !string.IsNullOrEmpty(this.TraefikApiPort)),
                    (nameof(this.WaykDenPort), !string.IsNullOrEmpty(this.WaykDenPort)),
                    (nameof(this.CertificatePath), !string.IsNullOrEmpty(this.CertificatePath)),
                    (nameof(this.KeyPath), !string.IsNullOrEmpty(this.KeyPath)),
                    (nameof(this.MongoImage), !string.IsNullOrEmpty(this.MongoImage)),
                    (nameof(this.PickyImage), !string.IsNullOrEmpty(this.PickyImage)),
                    (nameof(this.DenLucidImage), !string.IsNullOrEmpty(this.DenLucidImage)),
                    (nameof(this.DenRouterImage), !string.IsNullOrEmpty(this.DenRouterImage)),
                    (nameof(this.DenServerImage), !string.IsNullOrEmpty(this.DenServerImage)),
                    (nameof(this.TraefikImage), !string.IsNullOrEmpty(this.TraefikImage)),
                    (nameof(this.DevolutionsJetImage), !string.IsNullOrEmpty(this.DevolutionsJetImage))
                };

                (string, bool)[] names = values.Where(x => x.Item2.Equals(true)).ToArray();

                if(names.Length == 0 && this.Remove == null)
                {
                    return;
                }

                DenConfigController denConfigController = new DenConfigController(this.Path, this.Key);
                DenConfig config = denConfigController.GetConfig();

                if(this.Remove != null)
                {
                    foreach(string name in this.Remove)
                    {
                        bool ok = this.dictionary.TryGetValue(name, out var value);

                        if(!ok)
                        {
                            continue;
                        }

                        var property = config.GetType().GetProperty(value.Item1.Name).GetValue(config);
                        property.GetType().GetProperty(value.Item2).SetValue(property, null);
                    }
                }
                else
                {
                    foreach((string, bool) name in names)
                    {
                        bool ok = this.dictionary.TryGetValue(name.Item1, out var value);
                        if(!ok)
                        {
                            continue;
                        }

                        var property = config.GetType().GetProperty(value.Item1.Name).GetValue(config);
                        var newValue = this.GetType().GetProperty(name.Item1).GetValue(this);
                        property.GetType().GetProperty(value.Item2).SetValue(property, newValue);
                    }
                }

                denConfigController.StoreConfig(config);
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }
    }
}