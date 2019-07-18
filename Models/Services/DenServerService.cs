using WaykDen.Utils;
using WaykDen.Controllers;

namespace WaykDen.Models.Services
{
    public class DenServerService : DenService
    {
        public const string DENSERVER_NAME = "den-server";
        private const string DENSERVER_IMAGE = "devolutions/waykden-rs:1.1.0-dev";
        private const string DEN_PRIVATE_KEY_DATA_ENV = "DEN_PRIVATE_KEY_DATA";
        private const string PICKY_REALM_ENV = "PICKY_REALM";
        private const string PICKY_URL_ENV = "PICKY_URL";
        private const string PICKY_API_KEY_ENV = "PICKY_APIKEY";
        private const string AUDIT_TRAILS_ENV = "AUDIT_TRAILS";
        private const string LUCID_AUTHENTICATION_KEY_ENV = "LUCID_AUTHENTICATION_KEY";
        private const string ROUTER_INTERNAL_URL_ENV = "DEN_ROUTER_INTERNAL_URL";
        private const string ROUTER_EXTERNAL_URL_ENV = "DEN_ROUTER_EXTERNAL_URL";
        private const string LUCID_INTERNAL_URL_ENV = "LUCID_INTERNAL_URL";
        private const string LUCID_EXTERNAL_URL_ENV = "LUCID_EXTERNAL_URL";
        private const string JET_SERVER_URL_ENV = "JET_SERVER_URL";
        private const string LDAP_SERVER_URL_ENV = "LDAP_SERVER_URL";
        private const string LDAP_USERNAME_ENV = "LDAP_USERNAME";
        private const string LDAP_PASSWORD_ENV = "LDAP_PASSWORD";
        private const string LDAP_USER_GROUP_ENV = "LDAP_USER_GROUP";
        private const string LDAP_SERVER_TYPE_ENV = "LDAP_SERVER_TYPE";
        private const string LDAP_BASE_DN_ENV = "LDAP_BASE_DN";
        private const string PICKY_URL = "http://den-picky:12345";
        private const string ROUTER_INTERNAL_URL = "ws://den-router:4491";
        private const string LUCID_INTERNAL_URL = "http://den-lucid:4242";
        private const string DEFAULT_MONGO_URL = "mongodb://den-mongo";
        private const string DEFAULT_MONGO_PORT = "27017";
        private const string DEN_API_KEY_ENV = "DEN_API_KEY";
        private const string DEN_LOGIN_REQUIRED_ENV = "DEN_LOGIN_REQUIRED";
        public DenServerService(DenServicesController controller)
        {
            this.DenServicesController = controller;
            this.Name = DENSERVER_NAME;
            this.ImageName = this.DenConfig.DenImageConfigObject.DenServerImage;

            string dburl = string.Empty;
            if(string.IsNullOrEmpty(this.DenConfig.DenMongoConfigObject.Url))
            {
                if(this.DenConfig.DenMongoConfigObject.Port == string.Empty)
                {
                    dburl = $"{DEFAULT_MONGO_URL}:{DEFAULT_MONGO_PORT}";
                } else dburl = $"{DEFAULT_MONGO_URL}:{this.DenConfig.DenMongoConfigObject.Port}";
            } else {
                dburl = $"{this.DenConfig.DenMongoConfigObject.Url}:{this.DenConfig.DenMongoConfigObject.Port}";
            }

            string externalRouterUrl = this.DenConfig.DenServerConfigObject.ExternalUrl;
            if(externalRouterUrl.StartsWith("https"))
            {
                externalRouterUrl = externalRouterUrl.Replace("https", "wss");
            } else externalRouterUrl = externalRouterUrl.Replace("http", "ws");

            this.Env.Add($"{DEN_PRIVATE_KEY_DATA_ENV}={RsaKeyutils.DerToPem(this.DenConfig.DenServerConfigObject.PrivateKey)}");
            this.Env.Add($"{PICKY_REALM_ENV}={this.DenConfig.DenPickyConfigObject.Realm}");
            this.Env.Add($"{PICKY_URL_ENV}={PICKY_URL}");
            this.Env.Add($"{PICKY_API_KEY_ENV}={this.DenConfig.DenPickyConfigObject.ApiKey}");
            this.Env.Add($"{AUDIT_TRAILS_ENV}={this.DenConfig.DenServerConfigObject.AuditTrails}");
            this.Env.Add($"{LUCID_AUTHENTICATION_KEY_ENV}={this.DenConfig.DenLucidConfigObject.ApiKey}");
            this.Env.Add($"{ROUTER_INTERNAL_URL_ENV}={ROUTER_INTERNAL_URL}");
            this.Env.Add($"{ROUTER_EXTERNAL_URL_ENV}={externalRouterUrl}/cow");
            this.Env.Add($"{LUCID_INTERNAL_URL_ENV}={LUCID_INTERNAL_URL}");
            this.Env.Add($"{LUCID_EXTERNAL_URL_ENV}={this.DenConfig.DenServerConfigObject.ExternalUrl}/lucid");
            this.Env.Add($"{DEN_LOGIN_REQUIRED_ENV}={this.DenConfig.DenServerConfigObject.LoginRequired.ToLower()}");
            if(!string.IsNullOrEmpty(this.DenConfig.DenServerConfigObject.LDAPUsername))
            {
                this.Env.Add($"{LDAP_USERNAME_ENV}={this.DenConfig.DenServerConfigObject.LDAPUsername}");
            }

            if(!string.IsNullOrEmpty(this.DenConfig.DenServerConfigObject.LDAPPassword))
            {
                this.Env.Add($"{LDAP_PASSWORD_ENV}={this.DenConfig.DenServerConfigObject.LDAPPassword}");                
            }

            if(!string.IsNullOrEmpty(this.DenConfig.DenServerConfigObject.LDAPServerUrl))
            {
                this.Env.Add($"{LDAP_SERVER_URL_ENV}={this.DenConfig.DenServerConfigObject.LDAPServerUrl}");    
            }
            
            if(!string.IsNullOrEmpty(this.DenConfig.DenServerConfigObject.LDAPUserGroup))
            {
                this.Env.Add($"{LDAP_USER_GROUP_ENV}={this.DenConfig.DenServerConfigObject.LDAPUserGroup}");   
            }

            if(!string.IsNullOrEmpty(this.DenConfig.DenServerConfigObject.JetServerUrl))
            {
                this.Env.Add($"{JET_SERVER_URL_ENV}={this.DenConfig.DenServerConfigObject.JetServerUrl}");
            }

            if(!string.IsNullOrEmpty(this.DenConfig.DenServerConfigObject.LDAPBaseDN))
            {
                this.Env.Add($"{LDAP_BASE_DN_ENV}={this.DenConfig.DenServerConfigObject.LDAPBaseDN}");
            }

            if(!string.IsNullOrEmpty(this.DenConfig.DenServerConfigObject.LDAPServerType))
            {
                this.Env.Add($"{LDAP_SERVER_TYPE_ENV}={this.DenConfig.DenServerConfigObject.LDAPServerType}");
            }

            this.Env.Add($"{DEN_API_KEY_ENV}={this.DenConfig.DenServerConfigObject.ApiKey}");
            this.Cmd.Add("--db_url");
            this.Cmd.Add(dburl);
            this.Cmd.Add("-m");
            this.Cmd.Add("onprem");
            this.Cmd.Add("-l");
            this.Cmd.Add("trace");
        }
    }
}