using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Management.Automation;
using Newtonsoft.Json;

namespace WaykDen.Models
{
    public class DenObject
    {
        public string Property {get; set;}
        public string Value {get; set;}
    }

    public class DenMongoConfigObject
    {
        [JsonIgnore]
        public int Id {get; set;}
        public string Url {get; set;}
        [JsonIgnore]
        public bool IsExternal {get; set;}
    }

    public class DenPickyConfigObject
    {
        [JsonIgnore]
        public int Id {get; set;}
        public string ApiKey {get; set;}
        public string Realm {get; set;}
        public string Backend {get; set;}
    }

    public class DenLucidConfigObject
    {
        [JsonIgnore]
        public int Id {get; set;}
        public string ApiKey {get; set;}
        public string AdminSecret {get; set;}
        public string AdminUsername {get; set;}
    }

    public class DenRouterConfigObject
    {
        [JsonIgnore]
        public int Id {get; set;}
        [JsonIgnore]
        public byte[] PublicKey {get; set;}
    }

    public class DenServerConfigObject
    {
        [JsonIgnore]
        public int Id {get; set;}
        public string AuditTrails {get; set;}
        public string ApiKey {get; set;}
        [JsonIgnore]
        public byte[] PrivateKey {get; set;}
        public string ExternalUrl {get; set;}
        public string LDAPServerUrl {get; set;}
        public string LDAPUsername {get; set;}
        public string LDAPPassword {get; set;}
        public string LDAPUserGroup {get; set;}
        public string LDAPServerType {get; set;}
        public string LDAPBaseDN {get; set;}
        public string JetServerUrl {get; set;}
        public string JetRelayUrl {get; set;}
        public string LoginRequired {get; set;}
    }

    public class DenTraefikConfigObject
    {
        [JsonIgnore]
        public int Id {get; set;}
        [JsonIgnore]
        public string WaykDenPort {get; set;}
        [JsonIgnore]
        public string Certificate {get; set;}
        [JsonIgnore]
        public string PrivateKey {get; set;}
    }
    
    public enum Platforms
    {
        Windows,
        Linux
    }

    public class DenImageConfigObject
    {
        private const string MONGO = "mongo";
        private const string PICKY = "picky";
        private const string LUCID = "lucid";
        private const string ROUTER = "router";
        private const string SERVER = "server";
        private const string TRAEFIK = "traefik";
        private const string JET = "jet";

        public const string LinuxDenMongoImage = "library/mongo:4.1-bionic";
        public const string LinuxDenLucidImage = "devolutions/den-lucid:3.3.3-buster";
        public const string LinuxDenPickyImage = "devolutions/picky:3.0.0-buster";
        public const string LinuxDenRouterImage = "devolutions/den-router:0.5.0-buster";
        public const string LinuxDenServerImage = "devolutions/den-server:1.5.0-buster";
        public const string LinuxDenTraefikImage = "library/traefik:1.7";
        public const string LinuxDevolutionsJetImage = "devolutions/devolutions-jet:0.4.0-stretch";
        public const string WindowsDenMongoImage = "library/mongo:4.2.0-rc3-windowsservercore-ltsc2016";
        public const string WindowsDenLucidImage = "devolutions/den-lucid:3.3.3-servercore-ltsc2019";
        public const string WindowsDenPickyImage = "devolutions/picky:3.0.0-servercore-ltsc2019";
        public const string WindowsDenRouterImage = "devolutions/den-router:0.5.0-servercore-ltsc2019";
        public const string WindowsDenServerImage = "devolutions/den-server:1.5.0-servercore-ltsc2019";
        public const string WindowsDenTraefikImage = "sixeyed/traefik:v1.7.8-windowsservercore-ltsc2019";
        public const string WindowsDevolutionsJetImage = "devolutions/devolutions-jet:0.4.0-servercore-ltsc2019";

        [JsonIgnore]
        public int Id {get; set;}
        public string DenMongoImage {get; set;}
        public string DenPickyImage {get; set;}
        public string DenLucidImage {get; set;}
        public string DenRouterImage {get; set;}
        public string DenServerImage {get; set;}
        public string DenTraefikImage {get; set;}
        public string DevolutionsJetImage {get; set;}
        private Dictionary<Platforms, Dictionary<string, string>> images = new Dictionary<Platforms, Dictionary<string, string>>()
        {
            {
                Platforms.Linux, new Dictionary<string, string>()
                {
                    {MONGO, LinuxDenMongoImage},
                    {PICKY, LinuxDenPickyImage},
                    {LUCID, LinuxDenLucidImage},
                    {ROUTER, LinuxDenRouterImage},
                    {SERVER, LinuxDenServerImage},
                    {TRAEFIK, LinuxDenTraefikImage},
                    {JET, LinuxDevolutionsJetImage}
                }
            },

            {
                Platforms.Windows, new Dictionary<string, string>()
                {
                    {MONGO, WindowsDenMongoImage},
                    {PICKY, WindowsDenPickyImage},
                    {LUCID, WindowsDenLucidImage},
                    {ROUTER, WindowsDenRouterImage},
                    {SERVER, WindowsDenServerImage},
                    {TRAEFIK, WindowsDenTraefikImage},
                    {JET, WindowsDevolutionsJetImage}
                }
            }
        };
        public DenImageConfigObject(Platforms platform)
        {
            if(this.images.TryGetValue(platform, out Dictionary<string, string> images))
            {
                string mongoEnvironment = Environment.GetEnvironmentVariable("DEN_MONGO_IMAGE");
                string pickyEnvironment = Environment.GetEnvironmentVariable("DEN_PICKY_IMAGE");
                string lucidEnvironment = Environment.GetEnvironmentVariable("DEN_LUCID_IMAGE");
                string routeurEnvironment = Environment.GetEnvironmentVariable("DEN_ROUTER_IMAGE");
                string serverEnvironment = Environment.GetEnvironmentVariable("DEN_SERVER_IMAGE");
                string traefikEnvironment = Environment.GetEnvironmentVariable("DEN_TRAEFIK_IMAGE");
                string jetEnvironment = Environment.GetEnvironmentVariable("DEN_JET_IMAGE");

                if (!string.IsNullOrEmpty(mongoEnvironment)){
                    this.DenMongoImage = mongoEnvironment;
                }else{
                    this.DenMongoImage = images.TryGetValue(MONGO, out string mongo) ? mongo : throw new Exception("Could not find image for Mongodb");
                }if (!string.IsNullOrEmpty(pickyEnvironment)){
                    this.DenPickyImage = pickyEnvironment;
                }else{
                    this.DenPickyImage = images.TryGetValue(PICKY, out string picky) ? picky : throw new Exception("Could not find image for Den-picky");
                }if (!string.IsNullOrEmpty(lucidEnvironment)){
                    this.DenLucidImage = lucidEnvironment;
                }else {
                    this.DenLucidImage = images.TryGetValue(LUCID, out string lucid) ? lucid : throw new Exception("Could not find image for Den-lucid");
                }if (!string.IsNullOrEmpty(routeurEnvironment)) {
                    this.DenRouterImage = routeurEnvironment;
                }else{
                    this.DenRouterImage = images.TryGetValue(ROUTER, out string router) ? router : throw new Exception("Could not find image for Den-router");
                }if (!string.IsNullOrEmpty(serverEnvironment)){
                    this.DenServerImage = serverEnvironment;
                }else {
                    this.DenServerImage = images.TryGetValue(SERVER, out string server) ? server : throw new Exception("Could not find image for Den-server");
                }if (!string.IsNullOrEmpty(traefikEnvironment)){
                    this.DenTraefikImage = traefikEnvironment;
                }else{
                    this.DenTraefikImage = images.TryGetValue(TRAEFIK, out string traefik) ? traefik : throw new Exception("Could not find image for Traefik");
                }if (!string.IsNullOrEmpty(jetEnvironment)){
                    this.DevolutionsJetImage = jetEnvironment;
                }else{
                    this.DevolutionsJetImage = images.TryGetValue(JET, out string jet) ? jet : throw new Exception("Could not find image for DevolutionsJet");
                }
            }
        }
    }

    public class DenDockerConfigObject
    {
        [JsonIgnore]
        public int Id {get; set;}
        [JsonIgnore]
        public string DockerClientUri {get; set;}
        [JsonIgnore]
        public string Platform {get; set;}
        public string SyslogServer {get; set;}
    }
}