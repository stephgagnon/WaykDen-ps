using System;
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
        private const string WAYK_DEN_CONFIG_KEY = "WAYK_DEN_CONFIG_KEY";
        private const string DEN_IMAGE_CONFIG_COLLECTION = "DenImageConfig";
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
            this.TestConnString();
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
            
            if(!string.IsNullOrEmpty(this.password))
            {
                Environment.SetEnvironmentVariable(WAYK_DEN_CONFIG_KEY, this.password);
            }
        }

        public bool DbExists
        {
            get
            {
                using(var db = new LiteDatabase(this.connString))
                {
                    var collections = db.GetCollectionNames().ToArray();
                    return collections.Length > 0;
                }
            }
        }

        public void StoreConfig(DenConfig config)
        {
            using(var db = new LiteDatabase(this.connString))
            {
                if(db.CollectionExists(DEN_IMAGE_CONFIG_COLLECTION))
                {
                    this.Update(db, config);
                } else this.Store(db ,config);
            }
        }

        private void Store(LiteDatabase db, DenConfig config)
        {
            this.StoreImage(db, config.DenImageConfigObject);
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
            this.UpdateImage(db, config.DenImageConfigObject);
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
                throw new Exception("Could not found WaykDen configuration in given path. Make sure WaykDen configuration is in current folder or set WAYK_DEN_HOME to the path of WaykDen configuration");
            }

            using(var db = new LiteDatabase(this.connString))
            {
                return new DenConfig()
                {
                    DenImageConfigObject = this.GetImage(db),
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

        private DenImageConfigObject GetImage(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_IMAGE_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            values.TryGetValue(nameof(DenImageConfigObject.DenMongoImage), out var mongo);
            values.TryGetValue(nameof(DenImageConfigObject.DenLucidImage), out var lucid);
            values.TryGetValue(nameof(DenImageConfigObject.DenPickyImage), out var picky);
            values.TryGetValue(nameof(DenImageConfigObject.DenRouterImage), out var router);
            values.TryGetValue(nameof(DenImageConfigObject.DenServerImage), out var server);
            values.TryGetValue(nameof(DenImageConfigObject.DenTraefikImage), out var traefik);
            values.TryGetValue(nameof(DenImageConfigObject.DevolutionsJetImage), out var jet);
            return new DenImageConfigObject()
            {
                DenMongoImage = mongo?.ToString().Trim('\"'),
                DenLucidImage = lucid?.ToString().Trim('\"'),
                DenPickyImage = picky?.ToString().Trim('\"'),
                DenRouterImage = router?.ToString().Trim('\"'),
                DenServerImage = server?.ToString().Trim('\"'),
                DenTraefikImage = traefik?.ToString().Trim('\"'),
                DevolutionsJetImage = jet?.ToString().Trim('\"')
            };
        }

        private DenMongoConfigObject GetMongo(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_MONGO_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            values.TryGetValue(nameof(DenMongoConfigObject.Url), out var url);
            values.TryGetValue(nameof(DenMongoConfigObject.Port), out var port);
            return new DenMongoConfigObject()
            {
                Url = url?.ToString().Trim('\"'),
                Port = port?.ToString().Trim('\"')
            };
        }

        private DenPickyConfigObject GetPicky(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_PICKY_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            values.TryGetValue(nameof(DenPickyConfigObject.Realm), out var realm);
            values.TryGetValue(nameof(DenPickyConfigObject.ApiKey), out var apikey);
            values.TryGetValue(nameof(DenPickyConfigObject.Backend), out var backend);
            return new DenPickyConfigObject()
            {
                Realm = realm?.ToString().Trim('\"'),
                ApiKey = apikey?.ToString().Trim('\"'),
                Backend = backend?.ToString().Trim('\"')
            };
        }

        private DenLucidConfigObject GetLucid(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_LUCID_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            values.TryGetValue(nameof(DenLucidConfigObject.ApiKey), out var apikey);
            values.TryGetValue(nameof(DenLucidConfigObject.AdminSecret), out var adminsecret);
            values.TryGetValue(nameof(DenLucidConfigObject.AdminUsername), out var adminusername);
            return new DenLucidConfigObject()
            {
                ApiKey = apikey?.ToString().Trim('\"'),
                AdminSecret = adminsecret?.ToString().Trim('\"'),
                AdminUsername = adminusername?.ToString().Trim('\"')
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
            values.TryGetValue(nameof(DenServerConfigObject.ApiKey), out var apikey);
            values.TryGetValue(nameof(DenServerConfigObject.AuditTrails), out var auditTrails);
            values.TryGetValue(nameof(DenServerConfigObject.ExternalUrl), out var externalUrl);
            values.TryGetValue(nameof(DenServerConfigObject.LDAPServerType), out var ldapservertype);
            values.TryGetValue(nameof(DenServerConfigObject.LDAPPassword), out var ldappassword);
            values.TryGetValue(nameof(DenServerConfigObject.LDAPServerUrl), out var ldapserverurl);
            values.TryGetValue(nameof(DenServerConfigObject.LDAPUserGroup), out var ldapusergroup);
            values.TryGetValue(nameof(DenServerConfigObject.LDAPUsername), out var ldapusername);
            values.TryGetValue(nameof(DenServerConfigObject.LDAPBaseDN), out var ldapbasedn);
            values.TryGetValue(nameof(DenServerConfigObject.PrivateKey), out var privatekey);
            values.TryGetValue(nameof(DenServerConfigObject.JetServerUrl), out var jetServerUrl);
            return new DenServerConfigObject()
            {
                ApiKey = apikey?.ToString().Trim('\"'),
                AuditTrails = auditTrails?.ToString().Trim('\"'),
                ExternalUrl = externalUrl?.ToString().Trim('\"'),
                LDAPServerType = ldapservertype?.ToString().Trim('\"'),
                LDAPBaseDN = ldapbasedn?.ToString().Trim('\"'),
                LDAPPassword = ldappassword?.ToString().Trim('\"'),
                LDAPServerUrl = ldapserverurl?.ToString().Trim('\"'),
                LDAPUserGroup = ldapusergroup?.ToString().Trim('\"'),
                LDAPUsername = ldapusername?.ToString().Trim('\"'),
                PrivateKey = privatekey,
                JetServerUrl = jetServerUrl?.ToString().Trim('\"')
            };
        }

        private DenTraefikConfigObject GetTraefik(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_TRAEFIK_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            values.TryGetValue(nameof(DenTraefikConfigObject.ApiPort), out var apiPort);
            values.TryGetValue(nameof(DenTraefikConfigObject.WaykDenPort), out var waykDenPort);
            values.TryGetValue(nameof(DenTraefikConfigObject.Certificate), out var certificate);
            values.TryGetValue(nameof(DenTraefikConfigObject.PrivateKey), out var privateKey);
            return new DenTraefikConfigObject
            {
                ApiPort = apiPort,
                WaykDenPort = waykDenPort,
                Certificate = certificate,
                PrivateKey = privateKey
            };
        }

        private DenDockerConfigObject GetDocker(LiteDatabase db)
        {
            var coll = db.GetCollection(DEN_DOCKER_CONFIG_COLLECTION);
            var values = coll.FindById(DB_ID);
            values.TryGetValue(nameof(DenDockerConfigObject.DockerClientUri), out var dockerclienturi);
            return new DenDockerConfigObject()
            {
                DockerClientUri = dockerclienturi.ToString().Trim('\"')
            };
        }

        private void StoreImage(LiteDatabase db, DenImageConfigObject images)
        {
            var col = db.GetCollection<DenImageConfigObject>(DEN_IMAGE_CONFIG_COLLECTION);
            col.Insert(DB_ID, images);
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

        private void UpdateImage(LiteDatabase db, DenImageConfigObject images)
        {
            var col = db.GetCollection<DenImageConfigObject>(DEN_IMAGE_CONFIG_COLLECTION);
            col.Update(DB_ID, images);
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
                    string pswd = DenServiceUtils.Generate(20);
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
                this.password = null;
                Environment.SetEnvironmentVariable(WAYK_DEN_CONFIG_KEY, this.password);
            }
        }

        public void ChangeConfigKey(string newKey, string oldKey)
        {
            using(var db = new LiteDatabase($"Filename={path}; Password={oldKey}; Mode=Exclusive"))
            {
                db.Engine.Shrink(newKey);
                this.password = newKey;
                Environment.SetEnvironmentVariable(WAYK_DEN_CONFIG_KEY, this.password);
            }
        }
    }
}