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
    }

    public class DenTraefikConfigObject
    {
        public int Id {get; set;}
        public string ApiPort {get; set;}
        public string WaykDenPort {get; set;}
        public string Certificate {get; set;}
        public string PrivateKey {get; set;}
    }

    public class DenImageConfigObject
    {
        public int Id {get; set;}
        public string DenMongoImage {get; set;}
        public string DenPickyImage {get; set;}
        public string DenLucidImage {get; set;}
        public string DenRouterImage {get; set;}
        public string DenServerImage {get; set;}
        public string DenTraefikImage {get; set;}
        public string DevolutionsJetImage {get; set;}
    }

    public class DenDockerConfigObject
    {
        public int Id {get; set;}
        public string DockerClientUri {get; set;}
    }
}
