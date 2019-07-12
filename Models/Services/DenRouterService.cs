using WaykDen.Utils;
using WaykDen.Controllers;

namespace WaykDen.Models.Services
{
    public class DenRouterService : DenService
    {
        public const string DENROUTER_NAME = "den-router";
        private const string DEN_PUBLIC_KEY_DATA_ENV = "DEN_PUBLIC_KEY_DATA";

        public DenRouterService(DenServicesController controller)
        {
            this.DenServicesController = controller;
            this.Name = DENROUTER_NAME;
            this.ImageName = this.DenConfig.DenImageConfigObject.DenRouterImage;
            this.Env.Add($"{DEN_PUBLIC_KEY_DATA_ENV}={RsaKeyutils.DerToPem(this.DenConfig.DenRouterConfigObject.PublicKey)}");
        }
    }
}