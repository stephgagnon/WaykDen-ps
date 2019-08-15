using System;
using System.Threading;
using System.Threading.Tasks;
using WaykDen.Controllers;
using Docker.DotNet.Models;

namespace WaykDen.Models.Services
{
    public class DenHealthCheckService : DenService
    {
        public DenHealthCheckService(DenServicesController controller, string name) : base(controller, name)
        {
            this.HealthCheck.Add("CMD-SHELL");
        }
        public async override Task<bool> IsHealthy()
        {
            int i = 0;
            ContainerInspectResponse cir = await this.DockerClient.Containers.InspectContainerAsync(this.Container_ID);
            bool running = await this.IsRunning();
            bool healthy = cir.State.Health.Status == "healthy";
            
            while((i < 30) &&  !healthy && running)
            {
                running = await this.IsRunning();
                cir = await this.DockerClient.Containers.InspectContainerAsync(this.Container_ID);
                healthy = cir.State.Health.Status == "healthy";
                i++;
                Thread.Sleep(1000);
            }

            return cir.State.Health.Status == "healthy";
        }       
    }
}