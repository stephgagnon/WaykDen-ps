using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using WaykDen.Cmdlets;
using WaykDen.Models;
using WaykDen.Utils;

namespace WaykDen.Controllers
{
    public class DenConfigController
    {
        private const string DEFAULT_MONGO_URL = "mongodb://den-mongo:27017";
        private const string DEFAULT_JET_SERVER_URL = "jet.wayk.net:8080";
        private const string DEFAULT_JET_RELAY_URL = "https://jet.wayk.net";
        private const string DEN_MONGO_CONFIG_COLLECTION = "DenMongoConfig";
        private const string DEN_PICKY_CONFIG_COLLECTION = "DenPickyConfig";
        private const string DEN_LUCID_CONFIG_COLLECTION = "DenLucidConfig";
        private const string DEN_ROUTER_CONFIG_COLLECTION = "DenRouterConfig";
        private const string DEN_SERVER_CONFIG_COLLECTION = "DenServerConfig";
        private const string DEN_TRAEFIK_CONFIG_COLLECTION = "DenTraefikConfig";
        private const string DEN_DOCKER_CONFIG_COLLECTION = "DenDockerConfig";
        private const int DB_ID = 1;
        private string path;
        private string password;
        private string connString = string.Empty;
        public DenConfigController(string path, string password)
        {
            this.path = $"{path}/WaykDen.db";
            this.password = string.IsNullOrEmpty(password) ? string.Empty : password;
            this.connString = string.IsNullOrEmpty(password) ? $"Filename={this.path}; Mode=Exclusive" : $"Filename={this.path}; Password={this.password}; Mode=Exclusive";
            if(File.Exists(this.path))
            {
                this.TestConnString();
            }

            BsonMapper.Global.EmptyStringToNull = false;
        }

        private void TestConnString()
        {
            try
            {
                using(var db = new LiteDatabase(this.connString))
                {
                    var collections = db.GetCollectionNames();
                }
            }
            catch(Exception)
            {
                throw new Exception("Invalid database password.");
            }
        }

        public bool DbExists
        {
            get
            {
                if(!File.Exists(this.path))
                {
                    return false;
                }

                using(var db = new LiteDatabase(this.connString))
                {
                    var collections = db.GetCollectionNames().ToArray();
                    if(collections.Length > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
        }

        public void StoreConfig(DenConfig config)
        {
            using(var db = new LiteDatabase(this.connString))
            {
                if(db.CollectionExists(DEN_MONGO_CONFIG_COLLECTION))
                {
                    this.Update(db, config);
                } else this.Store(db ,config);
            }
        }

        private void Store(LiteDatabase db, DenConfig config)
        {
            this.StoreMongo(db, config.DenMongoConfigObject);
            this.StorePicky(db,config.DenPickyConfigObject);
            this.StoreLucid(db, config.DenLucidConfigObject);
            this.StoreRouter(db, config.DenRouterConfigObject);
            this.StoreServer(db, config.DenServerConfigObject);
            this.StoreTraefik(db, config.DenTraefikConfigObject);
            this.StoreDocker(db, config.DenDockerConfigObject);
        }

        private void Update(LiteDatabase db, DenConfig config)
        {
            this.UpdateMongo(db, config.DenMongoConfigObject);
            this.UpdatePicky(db, config.DenPickyConfigObject);
            this.UpdateLucid(db, config.DenLucidConfigObject);
            this.UpdateRouter(db, config.DenRouterConfigObject);
            this.UpdateServer(db, config.DenServerConfigObject);
            this.UpdateTraefik(db, config.DenTraefikConfigObject);
            this.UpdateDocker(db, config.DenDockerConfigObject);
        }

        public DenConfig GetConfig()
        {
            if(!this.DbExists)
            {
                throw new Exception("Could not find WaykDen configuration in given path. Use New-WaykDenConfig or make sure WaykDen configuration is in current folder or set WAYK_DEN_HOME to the path of WaykDen configuration");
            }

            using(var db = new LiteDatabase(this.connString))
            {
                return new DenConfig()
                {
                    DenLucidConfigObject = this.GetLucid(db),
                    DenPickyConfigObject = this.GetPicky(db),
                    DenMongoConfigObject = this.GetMongo(db),
                    DenRouterConfigObject = this.GetRouter(db),
                    DenServerConfigObject = this.GetServer(db),
                    DenTraefikConfigObject = this.GetTraefik(db),
                    DenDockerConfigObject = this.GetDocker(db)
                };
            }
        }

        private DenMongoConfigObject GetMongo(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_MONGO_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            bool urlOk = values.TryGetValue(nameof(DenMongoConfigObject.Url), out var url);
            url = string.IsNullOrEmpty((string)url) ? DEFAULT_MONGO_URL : ((string)url);
            return new DenMongoConfigObject()
            {
                Url = url,
                IsExternal = (string)url != DEFAULT_MONGO_URL
            };
        }

        private DenPickyConfigObject GetPicky(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_PICKY_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            bool realmOk = values.TryGetValue(nameof(DenPickyConfigObject.Realm), out var realm);
            bool apiKeyOk = values.TryGetValue(nameof(DenPickyConfigObject.ApiKey), out var apikey);
            bool backendOk = values.TryGetValue(nameof(DenPickyConfigObject.Backend), out var backend);
            return new DenPickyConfigObject()
            {
                Realm = realmOk ? ((string)realm) : string.Empty,
                ApiKey = apiKeyOk ? ((string)apikey) : string.Empty,
                Backend = backendOk ? ((string)backend) : string.Empty
            };
        }

        private DenLucidConfigObject GetLucid(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_LUCID_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            bool apiKeyOk = values.TryGetValue(nameof(DenLucidConfigObject.ApiKey), out var apikey);
            bool adminSecretOk = values.TryGetValue(nameof(DenLucidConfigObject.AdminSecret), out var adminsecret);
            bool adminUsernameOk = values.TryGetValue(nameof(DenLucidConfigObject.AdminUsername), out var adminusername);
            return new DenLucidConfigObject()
            {
                ApiKey = apiKeyOk ? ((string)apikey) : string.Empty,
                AdminSecret = adminSecretOk ? ((string)adminsecret) : string.Empty,
                AdminUsername = adminUsernameOk ? ((string)adminusername) : string.Empty
            };
        }

        private DenRouterConfigObject GetRouter(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_ROUTER_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            values.TryGetValue(nameof(DenRouterConfigObject.PublicKey), out var pubkey);
            return new DenRouterConfigObject()
            {
                PublicKey = pubkey
            };
        }

        private DenServerConfigObject GetServer(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_SERVER_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            bool apiKeyOk = values.TryGetValue(nameof(DenServerConfigObject.ApiKey), out var apikey);
            bool auditTrailsOK = values.TryGetValue(nameof(DenServerConfigObject.AuditTrails), out var auditTrails);
            bool externalUrlOk = values.TryGetValue(nameof(DenServerConfigObject.ExternalUrl), out var externalUrl);
            bool ldapServerTypeOk = values.TryGetValue(nameof(DenServerConfigObject.LDAPServerType), out var ldapservertype);
            bool ldapPasswordOk = values.TryGetValue(nameof(DenServerConfigObject.LDAPPassword), out var ldappassword);
            bool ldapServerUrlOk = values.TryGetValue(nameof(DenServerConfigObject.LDAPServerUrl), out var ldapserverurl);
            bool ldapUserGroupOk = values.TryGetValue(nameof(DenServerConfigObject.LDAPUserGroup), out var ldapusergroup);
            bool ldapUsernameOk = values.TryGetValue(nameof(DenServerConfigObject.LDAPUsername), out var ldapusername);
            bool ldapBaseDnOk = values.TryGetValue(nameof(DenServerConfigObject.LDAPBaseDN), out var ldapbasedn);
            values.TryGetValue(nameof(DenServerConfigObject.PrivateKey), out var privatekey);
            bool jetServerUrlOk = values.TryGetValue(nameof(DenServerConfigObject.JetServerUrl), out var jetServerUrl);
            bool jetRelayUrlOk = values.TryGetValue(nameof(DenServerConfigObject.JetRelayUrl), out var jetRelayUrl);
            bool loginRequiredOk = values.TryGetValue(nameof(DenServerConfigObject.LoginRequired), out var loginRequired);
            return new DenServerConfigObject()
            {
                ApiKey = apiKeyOk ? ((string)apikey) : string.Empty,
                AuditTrails = auditTrailsOK ? ((string) auditTrails) : string.Empty,
                ExternalUrl = externalUrlOk ? ((string) externalUrl).TrimEnd('/') : string.Empty,
                LDAPServerType = ldapServerTypeOk ? ((string) ldapservertype) : string.Empty,
                LDAPBaseDN = ldapBaseDnOk? ((string) ldapbasedn) : string.Empty,
                LDAPPassword = ldapPasswordOk ? ((string) ldappassword) : string.Empty,
                LDAPServerUrl = ldapServerUrlOk ? ((string) ldapserverurl) : string.Empty,
                LDAPUserGroup = ldapUserGroupOk ? ((string) ldapusergroup) : string.Empty,
                LDAPUsername = ldapUsernameOk ? ((string) ldapusername) : string.Empty,
                PrivateKey = privatekey,
                JetServerUrl = jetServerUrlOk && !string.IsNullOrEmpty((string) jetServerUrl) ? ((string) jetServerUrl) : DEFAULT_JET_SERVER_URL,
                JetRelayUrl = jetRelayUrlOk && !string.IsNullOrEmpty((string) jetRelayUrl) ? ((string) jetRelayUrl) : DEFAULT_JET_RELAY_URL,
                LoginRequired = loginRequiredOk ? ((string) loginRequired) : "false"
            };
        }

        private DenTraefikConfigObject GetTraefik(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_TRAEFIK_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            bool waykDenPortOk =  values.TryGetValue(nameof(DenTraefikConfigObject.WaykDenPort), out var waykDenPort);
            bool certificateOk = values.TryGetValue(nameof(DenTraefikConfigObject.Certificate), out var certificate);
            bool privateKeyOk = values.TryGetValue(nameof(DenTraefikConfigObject.PrivateKey), out var privateKey);
            return new DenTraefikConfigObject
            {
                WaykDenPort = waykDenPortOk ? ((string)waykDenPort) : "4000",
                Certificate = certificateOk ? (string)certificate : string.Empty,
                PrivateKey = privateKeyOk ? (string)privateKey : string.Empty
            };
        }

        private DenDockerConfigObject GetDocker(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_DOCKER_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            bool dockerclientUriOk = values.TryGetValue(nameof(DenDockerConfigObject.DockerClientUri), out var dockerclienturi);
            bool platformOk = values.TryGetValue(nameof(DenDockerConfigObject.Platform), out var platform);
            bool syslogOk = values.TryGetValue(nameof(DenDockerConfigObject.SyslogServer), out var syslog);
            return new DenDockerConfigObject()
            {
                DockerClientUri = dockerclientUriOk ? ((string) dockerclienturi) : string.Empty,
                Platform = platformOk ? ((string)platform) : "Linux",
                SyslogServer = syslogOk ? ((string)syslog) : string.Empty
            };
        }

        private void StoreMongo(LiteDatabase db, DenMongoConfigObject mongo)
        {
            var col = db.GetCollection<DenMongoConfigObject>(DEN_MONGO_CONFIG_COLLECTION);
            col.Insert(DB_ID, mongo);
        }

        private void StorePicky(LiteDatabase db, DenPickyConfigObject picky)
        {
            var col = db.GetCollection<DenPickyConfigObject>(DEN_PICKY_CONFIG_COLLECTION);
            col.Insert(DB_ID, picky);
        }

        private void StoreLucid(LiteDatabase db, DenLucidConfigObject lucid)
        {
            var col = db.GetCollection<DenLucidConfigObject>(DEN_LUCID_CONFIG_COLLECTION);
            col.Insert(DB_ID, lucid);
        }

        private void StoreRouter(LiteDatabase db, DenRouterConfigObject router)
        {
            var col = db.GetCollection<DenRouterConfigObject>(DEN_ROUTER_CONFIG_COLLECTION);
            col.Insert(DB_ID, router);
        }

        private void StoreServer(LiteDatabase db, DenServerConfigObject server)
        {
            var col = db.GetCollection<DenServerConfigObject>(DEN_SERVER_CONFIG_COLLECTION);
            col.Insert(DB_ID, server);
        }

        private void StoreTraefik(LiteDatabase db, DenTraefikConfigObject traefik)
        {
            var col = db.GetCollection<DenTraefikConfigObject>(DEN_TRAEFIK_CONFIG_COLLECTION);
            col.Insert(DB_ID, traefik);
        }

        private void StoreDocker(LiteDatabase db, DenDockerConfigObject docker)
        {
            var col = db.GetCollection<DenDockerConfigObject>(DEN_DOCKER_CONFIG_COLLECTION);
            col.Insert(DB_ID, docker);
        }

        private void UpdateMongo(LiteDatabase db, DenMongoConfigObject mongo)
        {
            var col = db.GetCollection<DenMongoConfigObject>(DEN_MONGO_CONFIG_COLLECTION);
            col.Update(DB_ID, mongo);
        }

        private void UpdatePicky(LiteDatabase db, DenPickyConfigObject picky)
        {
            var col = db.GetCollection<DenPickyConfigObject>(DEN_PICKY_CONFIG_COLLECTION);
            col.Update(DB_ID, picky);
        }

        private void UpdateLucid(LiteDatabase db, DenLucidConfigObject lucid)
        {
            var col = db.GetCollection<DenLucidConfigObject>(DEN_LUCID_CONFIG_COLLECTION);
            col.Update(DB_ID, lucid);
        }

        private void UpdateRouter(LiteDatabase db, DenRouterConfigObject router)
        {
            var col = db.GetCollection<DenRouterConfigObject>(DEN_ROUTER_CONFIG_COLLECTION);
            col.Update(DB_ID, router);
        }

        private void UpdateServer(LiteDatabase db, DenServerConfigObject server)
        {
            var col = db.GetCollection<DenServerConfigObject>(DEN_SERVER_CONFIG_COLLECTION);
            col.Update(DB_ID, server);
        }

        private void UpdateTraefik(LiteDatabase db, DenTraefikConfigObject traefik)
        {
            var col = db.GetCollection<DenTraefikConfigObject>(DEN_TRAEFIK_CONFIG_COLLECTION);
            col.Update(DB_ID, traefik);
        }

        private void UpdateDocker(LiteDatabase db, DenDockerConfigObject docker)
        {
            var col = db.GetCollection<DenDockerConfigObject>(DEN_DOCKER_CONFIG_COLLECTION);
            col.Update(DB_ID, docker);
        }

        private string LoadPassword()
        {
            try
            {
                string dir = Directory.GetParent(this.path).FullName;
                string file = $"{dir}/WaykDen.key";
                if(!File.Exists(file))
                {
                    string pswd = DenServiceUtils.GenerateRandom(20);
                    File.WriteAllText(file, pswd.Replace("-", string.Empty));
                    return pswd;
                }

                return File.ReadAllText(file);
            }
            catch(Exception e)
            {
                e.ToString();
                return string.Empty;
            }
        }

        public void AddConfigKey(string newKey)
        {
            using(var db = new LiteDatabase($"Filename={path}; Mode=Exclusive"))
            {
                db.Engine.Shrink(newKey);
            }
        }

        public void RemoveConfigKey(string key)
        {
            using(var db = new LiteDatabase($"Filename={path}; Password={key}; Mode=Exclusive"))
            {
                db.Engine.Shrink();
            }
        }

        public void ChangeConfigKey(string newKey, string oldKey)
        {
            using(var db = new LiteDatabase($"Filename={path}; Password={oldKey}; Mode=Exclusive"))
            {
                db.Engine.Shrink(newKey);
            }
        }
    }
}