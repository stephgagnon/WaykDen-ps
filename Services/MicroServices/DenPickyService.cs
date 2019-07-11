using System.Collections.Generic;
using System.Management.Automation;
using WaykPS.Controllers;

namespace WaykPS.Services.MicroServices
{
    [Cmdlet("Start", "Picky")]
    public class DenPickyService : DenService
    {
        public const string DENPICKY_NAME = "den-picky";
        private const string DEFAULT_MONGO_URL = "mongodb://den-mongo";
        private const string DEFAULT_MONGO_PORT = "27017";
        private const string PICKY_DB_URL_ENV = "PICKY_DATABASE_URL";
        private const string PICKY_API_KEY_ENV = "PICKY_API_KEY";
        private const string PICKY_REALM_ENV = "PICKY_REALM";

        public DenPickyService(DenServicesController controller)
        {
            this.DenServicesController = controller;
            this.Name = DENPICKY_NAME;
            this.ImageName = this.DenConfig.DenImageConfigObject.DenPickyImage;

            List<string> env = new List<string>{
                $"{PICKY_REALM_ENV}={this.DenConfig.DenPickyConfigObject.Realm}",
                $"{PICKY_API_KEY_ENV}={this.DenConfig.DenPickyConfigObject.ApiKey}"};

            if(string.IsNullOrEmpty(this.DenConfig.DenMongoConfigObject.Url))
            {
                if(this.DenConfig.DenMongoConfigObject.Port == string.Empty)
                {
                    env.Add($"{PICKY_DB_URL_ENV}={DEFAULT_MONGO_URL}:{DEFAULT_MONGO_PORT}");
                } else env.Add($"{PICKY_DB_URL_ENV}={DEFAULT_MONGO_URL}:{this.DenConfig.DenMongoConfigObject.Port}");
            } else {
                env.Add($"{PICKY_DB_URL_ENV}={this.DenConfig.DenMongoConfigObject.Url}:{this.DenConfig.DenMongoConfigObject.Port}");
            }

            this.Env = env;
        }
    }
}