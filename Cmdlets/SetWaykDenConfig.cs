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
        private const string WAYK_DEN_HOME = "WAYK_DEN_HOME";
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
        private Dictionary<string, (Type, string)> dictionary;
        public SetWaykDenConfig()
        {
            this.dictionary = new Dictionary<string, (Type, string)>
            {
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
                {nameof(this.DockerClientUri), (typeof(DenDockerConfigObject), "DockerClientUri")},
                {nameof(this.TraefikApiPort), (typeof(DenTraefikConfigObject), "ApiPort")},
                {nameof(this.WaykDenPort), (typeof(DenTraefikConfigObject), "WaykDenPort")},
                {nameof(this.CertificatePath), (typeof(DenTraefikConfigObject), "CertificatePath")},
                {nameof(this.KeyPath), (typeof(DenTraefikConfigObject), "KeyPath")},
                {nameof(this.MongoImage), (typeof(DenImageConfigObject), "DenMongoImage")},
                {nameof(this.PickyImage), (typeof(DenImageConfigObject), "DenPickyImage")},
                {nameof(this.DenLucidImage), (typeof(DenImageConfigObject), "DenLucidImage")},
                {nameof(this.DenRouterImage), (typeof(DenImageConfigObject), "DenRouterImage")},
                {nameof(this.DenServerImage), (typeof(DenImageConfigObject), "DenServerImage")},
                {nameof(this.TraefikImage), (typeof(DenImageConfigObject), "DenTraefikImage")}
            };
        }

        protected override void ProcessRecord()
        {
            try
            {
                (string, bool)[] values = new (string, bool)[]
                {
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
                    (nameof(this.DockerClientUri), !string.IsNullOrEmpty(this.DockerClientUri)),
                    (nameof(this.TraefikApiPort), !string.IsNullOrEmpty(this.TraefikApiPort)),
                    (nameof(this.WaykDenPort), !string.IsNullOrEmpty(this.WaykDenPort)),
                    (nameof(this.CertificatePath), !string.IsNullOrEmpty(this.CertificatePath)),
                    (nameof(this.KeyPath), !string.IsNullOrEmpty(this.KeyPath)),
                    (nameof(this.MongoImage), !string.IsNullOrEmpty(this.MongoImage)),
                    (nameof(this.PickyImage), !string.IsNullOrEmpty(this.PickyImage)),
                    (nameof(this.DenLucidImage), !string.IsNullOrEmpty(this.DenLucidImage)),
                    (nameof(this.DenRouterImage), !string.IsNullOrEmpty(this.DenRouterImage)),
                    (nameof(this.DenServerImage), !string.IsNullOrEmpty(this.DenServerImage)),
                    (nameof(this.TraefikImage), !string.IsNullOrEmpty(this.TraefikImage))
                };

                (string, bool)[] names = values.Where(x => x.Item2.Equals(true)).ToArray();

                if(names.Length == 0)
                {
                    return;
                }

                DenConfigController denConfigController = new DenConfigController(this.Path, this.Key);
                DenConfig config = denConfigController.GetConfig();

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

                denConfigController.StoreConfig(config);
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }
    }
}