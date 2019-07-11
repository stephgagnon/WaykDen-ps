using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using WaykPS.Config;

namespace WaykPS.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "WaykDenConfig")]
    public class RemoveWaykDenConfig : baseCmdlet
    {
        private const string WAYK_DEN_HOME = "WAYK_DEN_HOME";
        [Parameter]
        public string Path {get; set;} = string.Empty;
        [Parameter]
        public SwitchParameter MongoUrl {get; set;}
        [Parameter]
        public SwitchParameter MongoPort {get; set;}
        [Parameter]
        public SwitchParameter Realm {get; set;}
        [Parameter]
        public SwitchParameter ExternalUrl {get; set;}
        [Parameter]
        public SwitchParameter LDAPServerUrl {get; set;}
        [Parameter]
        public SwitchParameter LDAPUsername {get; set;}
        [Parameter]
        public SwitchParameter LDAPPassword {get; set;}
        [Parameter]
        public SwitchParameter LDAPUserGroup {get; set;}
        [Parameter]
        public SwitchParameter DockerClientUri {get; set;}
        private Dictionary<string, (Type, string)> dictionary;
        public RemoveWaykDenConfig()
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
                {nameof(this.DockerClientUri), (typeof(DenDockerConfigObject), "DockerClientUri")}
            };
        }

        protected override void ProcessRecord()
        {
            this.Path = Environment.GetEnvironmentVariable(WAYK_DEN_HOME);
            if(string.IsNullOrEmpty(this.Path))
            {
                this.Path = this.SessionState.Path.CurrentLocation.Path;
            }

            try
            {
                (string, bool)[] values = new (string, bool)[]
                {
                    (nameof(this.MongoUrl), this.MongoUrl.ToBool()),
                    (nameof(this.MongoPort), this.MongoPort.ToBool()),
                    (nameof(this.Realm), this.Realm.ToBool()),
                    (nameof(this.ExternalUrl), this.ExternalUrl.ToBool()),
                    (nameof(this.LDAPPassword), this.LDAPPassword.ToBool()),
                    (nameof(this.LDAPServerUrl), this.LDAPServerUrl.ToBool()),
                    (nameof(this.LDAPUserGroup), this.LDAPUserGroup.ToBool()),
                    (nameof(this.LDAPUsername), this.LDAPUsername.ToBool()),
                    (nameof(this.DockerClientUri), this.DockerClientUri.ToBool())
                };

                (string, bool)[] res = values.Where(x => x.Item2.Equals(true)).ToArray();

                DenConfigStore store = new DenConfigStore($"{this.Path}/WaykDen.db");
                DenConfigs configs = store.GetConfig();

                if(res.Length > 0)
                {
                    foreach((string, bool) name in res)
                    {
                        bool ok = this.dictionary.TryGetValue(name.Item1, out var value);

                        if(!ok)
                        {
                            return;
                        }

                        var property = configs.GetType().GetProperty(value.Item1.Name).GetValue(configs);
                        property.GetType().GetProperty(value.Item2).SetValue(property, "");
                    }  

                    store.StoreConfig(new DenConfig{DenConfigs = configs}); 
                }
            }
            catch(Exception e)
            {
                this.OnError(e);
            }
        }
    }
}