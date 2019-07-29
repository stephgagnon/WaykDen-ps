using System.Threading.Tasks;
using Docker.DotNet.Models;
using WaykDen.Controllers;

namespace WaykDen.Models.Services
{
    public class DenMongoService : DenService
    {
        public const string MONGO_NAME = "den-mongo";
        private const string MONGO_IMAGE = "mongo";
        private const string MONGO_LINUX_PATH = "/data/db";
        private const string MONGO_WINDOWS_PATH = "c:\\data\\db";
        private const string DEFAULT_MONGO_URL = "mongodb://den-mongo";
        public DenMongoService()
        {
        }

        public DenMongoService(DenServicesController controller) : base()
        {
            this.DenServicesController = controller;
            this.Name = MONGO_NAME;
            this.ImageName = this.DenConfig.DenImageConfigObject.DenMongoImage;
            Task.Run(async delegate
            {
                if(!await this.CheckVolumeExist())
                {
                    VolumeResponse volume = await this.CreateVolume();
                }
            });

            string mountPoint = this.DenConfig.DenDockerConfigObject.Platform == Platforms.Linux.ToString() ? MONGO_LINUX_PATH : MONGO_WINDOWS_PATH;

            this.Volumes.Add($"{this.Name}data:{mountPoint}");
        }

        public bool IsExternal => this.DenConfig.DenMongoConfigObject.Url != DEFAULT_MONGO_URL && !string.IsNullOrEmpty(this.DenConfig.DenMongoConfigObject.Url);
    }
}