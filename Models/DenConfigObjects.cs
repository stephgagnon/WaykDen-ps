using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Management.Automation;

namespace WaykDen.Models
{
    public class DenObject
    {
        public string Property {get; set;}
        public string Value {get; set;}
    }

    public class DenMongoConfigObject
    {
        public int Id {get; set;}
        public string Url {get; set;}
        public string Port {get; set;}
    }

    public class DenPickyConfigObject
    {
        public int Id {get; set;}
        public string ApiKey {get; set;}
        public string Realm {get; set;}
        public string Backend {get; set;}
    }

    public class DenLucidConfigObject
    {
        public int Id {get; set;}
        public string ApiKey {get; set;}
        public string AdminSecret {get; set;}
        public string AdminUsername {get; set;}
    }

    public class DenRouterConfigObject
    {
        public int Id {get; set;}
        public byte[] PublicKey {get; set;}
    }

    public class DenServerConfigObject
    {
        public int Id {get; set;}
        public string AuditTrails {get; set;}
        public string ApiKey {get; set;}
        public byte[] PrivateKey {get; set;}
        public string ExternalUrl {get; set;}
        public string LDAPServerUrl {get; set;}
        public string LDAPUsername {get; set;}
        public string LDAPPassword {get; set;}
        public string LDAPUserGroup {get; set;}
        public string LDAPServerType {get; set;}
        public string LDAPBaseDN {get; set;}
        public string JetServerUrl {get; set;}
        public string LoginRequired {get; set;}
    }

    public class DenTraefikConfigObject
    {
        public int Id {get; set;}
        public string ApiPort {get; set;}
        public string WaykDenPort {get; set;}
        public string Certificate {get; set;}
        public string PrivateKey {get; set;}
    }
    
    public enum Platforms
    {
        Windows,
        Linux
    }

    public class DenImageConfigObject
    {
        private const string LinuxMongoImage = "library/mongo:4.1-bionic";
        private const string LinuxDenLucidImage = "devolutions/den-lucid:3.3.3-stretch-dev";
        private const string LinuxPickyImage = "devolutions/picky:3.0.0-stretch-dev";
        private const string LinuxDenRouterImage = "devolutions/den-router:0.5.0-stretch-dev";
        private const string LinuxDenServerImage = "devolutions/den-server:1.2.0-stretch-dev";
        private const string LinuxDenTraefikImage = "library/traefik:1.7";
        private const string LinuxDevolutionsJetImage = "devolutions/devolutions-jet:0.4.0-stretch";
        private const string WindowsMongoImage = "library/mongo:4.2.0-rc3-windowsservercore-ltsc2016";
        private const string WindowsDenLucidImage = "devolutions/den-lucid:3.3.3-servercore-ltsc2019-dev";
        private const string WindowsPickyImage = "devolutions/picky:3.0.0-servercore-ltsc2019-dev";
        private const string WindowsDenRouterImage = "devolutions/den-router:0.5.0-servercore-ltsc2019-dev";
        private const string WindowsDenServerImage = "devolutions/den-server:1.2.0-servercore-ltsc2019-dev";
        private const string WindowsDenTraefikImage = "sixeyed/traefik:v1.7.8-windowsservercore-ltsc2019";
        private const string WindowsDevolutionsJetImage = "devolutions/devolutions-jet:0.4.0-servercore-ltsc2019-dev";
        public int Id {get; set;}
        public string DenMongoImage {get; set;}
        public string DenPickyImage {get; set;}
        public string DenLucidImage {get; set;}
        public string DenRouterImage {get; set;}
        public string DenServerImage {get; set;}
        public string DenTraefikImage {get; set;}
        public string DevolutionsJetImage {get; set;}
        public DenImageConfigObject(Platforms platform)
        {
            if(platform == Platforms.Windows)
            {
                this.DenMongoImage = WindowsMongoImage;
                this.DenPickyImage = WindowsPickyImage;
                this.DenLucidImage = WindowsDenLucidImage;
                this.DenRouterImage = WindowsDenRouterImage;
                this.DenServerImage = WindowsDenServerImage;
                this.DenTraefikImage = WindowsDenTraefikImage;
                this.DevolutionsJetImage = WindowsDevolutionsJetImage;
            }
            else
            {
                this.DenMongoImage = LinuxMongoImage;
                this.DenPickyImage = LinuxPickyImage;
                this.DenLucidImage = LinuxDenLucidImage;
                this.DenRouterImage = LinuxDenRouterImage;
                this.DenServerImage = LinuxDenServerImage;
                this.DenTraefikImage = LinuxDenTraefikImage;
                this.DevolutionsJetImage = LinuxDevolutionsJetImage;
            }
        }

        public DenImageConfigObject()
        {}
    }

    public class DenDockerConfigObject
    {
        public int Id {get; set;}
        public string DockerClientUri {get; set;}
        public string Platform {get; set;}
        public string SyslogServer {get; set;}
    }
}
