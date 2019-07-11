using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using WaykPS.Controllers;

namespace WaykPS.Services.MicroServices
{
    public class DevolutionsJetService : DenService
    {
        private const string JET_NAME = "devolutions-jet";
        private const string JET_INSTANCE_ENV = "JET_INSTANCE";
        public DevolutionsJetService(DenServicesController controller)
        {
            this.DenServicesController = controller;
            this.Name = JET_NAME;
            this.ImageName = this.DenConfig.DenImageConfigObject.DevolutionsJetImage;
            string[] splittedUrl = this.DenConfig.DenServerConfigObject.JetServerUrl.Split(':');
            string url = splittedUrl.Where(x => x.Contains('.')).FirstOrDefault().Replace("//", "");
            string port = splittedUrl.Last();
            this.Env.Add($"{JET_INSTANCE_ENV}={url}");
            this.Cmd.Add($"-u tcp://0.0.0.0:{port}");
            this.ExposedPorts.Add(port, new EmptyStruct());
            this.PortBindings.Add(port, new List<PortBinding>(){new PortBinding(){HostIP = "0.0.0.0", HostPort = port}});
        }

        public override async Task<CreateContainerResponse> CreateContainer(string image)
        {
            return await this.DockerClient.Containers.CreateContainerAsync
            (
                new CreateContainerParameters
                (
                    new Docker.DotNet.Models.Config()
                    {
                        Image = image,
                        Env = this.Env,
                        Cmd = this.Cmd,
                        ExposedPorts = this.ExposedPorts,
                        AttachStderr = false,
                        AttachStdin = false,
                        AttachStdout = true,
                    }
                )
                {
                    Name = this.Name,
                    HostConfig = new HostConfig()
                    {
                        PortBindings = this.PortBindings,
                        AutoRemove = true,
                        Binds = this.Volumes
                    }
                }
            );
        }
    }
}