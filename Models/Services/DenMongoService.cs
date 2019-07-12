using System.Threading.Tasks;
using Docker.DotNet.Models;
using WaykDen.Controllers;

namespace WaykDen.Models.Services
{
    public class DenMongoService : DenService
    {
        public const string MONGO_NAME = "den-mongo";
        private const string MONGO_IMAGE = "mongo";
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

            this.Volumes.Add($"{this.Name}data:/data/db");
        }
    }
}