using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using WaykDen.Utils;
using WaykDen.Models;
using WaykDen.Models.Services;
using WaykDen.Cmdlets;

namespace WaykDen.Controllers
{
    public class DenServicesController
    {
        private const string WAYK_DEN_HOME = "WAYK_DEN_HOME";
        private const string DENNETWORK_NAME = "den-network";
        public const string DOCKER_DEFAULT_CLIENT_URI_LINUX = "unix:///var/run/docker.sock";
        public const string DOCKER_DEFAULT_CLIENT_URI_WINDOWS = "npipe://./pipe/docker_engine";
        public string Path {get; set;} = string.Empty;
        public DenConfig DenConfig {get;}
        public DenNetwork DenNetwork {get;}
        public DockerClient DockerClient {get;}
        public List<DenService> RunningDenServices = new List<DenService>();
        public int ServicesCount;
        private DenConfigController denConfigController;
        private string dockerDefaultEndpoint
        {
            get
            {
                if(Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    return DOCKER_DEFAULT_CLIENT_URI_LINUX;
                } 
                else 
                {
                    return DOCKER_DEFAULT_CLIENT_URI_WINDOWS;
                }
            }
        }

        public DenServicesController(string path)
        {
            this.Path = Environment.GetEnvironmentVariable(WAYK_DEN_HOME);
            if(string.IsNullOrEmpty(this.Path))
            {
                this.Path = path;
            }

            this.Path = this.Path.EndsWith($"{System.IO.Path.DirectorySeparatorChar}") ? this.Path : $"{this.Path}{System.IO.Path.DirectorySeparatorChar}";
            try
            {
                this.denConfigController = new DenConfigController(this.Path);
                this.DenConfig = this.denConfigController.GetConfig();   
            }
            catch(Exception e)
            {
                if(this.OnError != null) this.OnError(e);
            }
            
            this.DenNetwork = new DenNetwork(this);
            
            if(string.IsNullOrEmpty(this.DenConfig.DenDockerConfigObject.DockerClientUri))
            {
                this.DenConfig.DenDockerConfigObject.DockerClientUri = this.dockerDefaultEndpoint;
            }
            this.DockerClient = new DockerClientConfiguration(new Uri(this.DenConfig.DenDockerConfigObject.DockerClientUri)).CreateClient();
        }

        private async Task<bool> StartDenMongo()
        {
            DenMongoService mongo = new DenMongoService(this);
            return await this.StartService(mongo);
        }

        private async Task<bool> StartDenPicky()
        {
            DenPickyService picky = new DenPickyService(this);
            return await this.StartService(picky);
        }

        private async Task<bool> StartDenLucid()
        {
            DenLucidService lucid = new DenLucidService(this);
            return await this.StartService(lucid);
        }

        private async Task<bool> StartDenRouter()
        {
            DenRouterService router = new DenRouterService(this);
            return await this.StartService(router);
        }

        private async Task<bool> StartDenServer()
        {
            DenServerService server = new DenServerService(this);
            return await this.StartService(server);
        }

        private async Task<bool> StartTraefikService()
        {
            DenTraefikService traefik = new DenTraefikService(this);
            bool started = await this.StartService(traefik);
            if(started)
            {
                Thread.Sleep(1000);
                await traefik.CurlTraefikConfig();
            }

            return started;
        }

        public async Task<bool> StartDevolutionsJet()
        {
            DevolutionsJetService jet = new DevolutionsJetService(this);
            bool started = await this.StartService(jet);
            this.denConfigController.StoreConfig(this.DenConfig);
            return started;
        }

        public async Task StopDevolutionsJet()
        {
            List<string> jet= await this.GetRunningContainer("devolutions-jet");
            if(jet.Count > 0)
            {
                foreach(string container in jet)
                {
                    await this.StopContainer(container);
                }
            }
        }

        private async Task<bool> StartService(DenService service)
        {
            string id = string.Empty;
            bool started = false;
            try
            {
                if(this.OnLog != null) this.OnLog($"{service.Name}");
                await service.CreateImage();

                CreateContainerResponse ccr = await service.CreateContainer(service.ImageName);
                if(this.OnLog != null) this.OnLog($"{service.Name} created");
                id = ccr.ID;

                if(this.OnLog != null) this.OnLog($"Starting {service.Name}");

                started = id != string.Empty? await service.StartContainer(id): false;

                if(this.OnLog != null) this.OnLog($"{service.Name} started");

                if(!started)
                {
                    if(this.OnLog != null) this.OnLog($"Error running {service.Name} container");
                } else {
                    this.RunningDenServices.Add(service);
                }
                
            }
            catch(DockerImageNotFoundException e)
            {
                this.OnError(e);
            }
            catch(DockerApiException e)
            {
                this.OnError(e);
            }

            return started;
        }

        private async Task<List<string>> CheckIfExists(string name)
        {
            List<string> containers = await this.GetRunningContainer(name);
            return containers;
        }

        private async Task<bool> StopContainer(string id)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            var t = Task.Run(async delegate
            {
                tcs.TrySetResult(await this.DockerClient.Containers.StopContainerAsync
                (
                    id,
                    new ContainerStopParameters()
                ));
            });

            bool result = await tcs.Task;
            return result;
        }

        public async Task StopDenNetwork(string id)
        {
            await this.DockerClient.Networks.DeleteNetworkAsync
            (
                id
            );
        }

        public async Task<List<string>> GetRunningContainer(string container_name = null)
        {
            if(this.DockerClient == null)
            {
                return null;
            }

            IDictionary<string, IDictionary<string, bool>> filter = new Dictionary<string, IDictionary<string, bool>>();
            if(container_name != null)
            {
                filter.Add("name", new Dictionary<string, bool>(){{container_name, true}});
            } else {
                filter.Add("network", new Dictionary<string, bool>(){{DENNETWORK_NAME, true}});
            }

            IList<ContainerListResponse> responses = await this.DockerClient.Containers.ListContainersAsync
            (
                new ContainersListParameters(){Filters = filter, All = true}
            );

            List<string> containerIds = new List<string>();

            foreach(ContainerListResponse response in responses)
            {
                containerIds.Add(response.ID);
            }

            this.ServicesCount = containerIds.Count;

            return containerIds;
        }

        public async Task<List<string>> GetDenNetwork()
        {
            if(this.DockerClient == null)
            {
                return null;
            }

            IDictionary<string, IDictionary<string, bool>> filter = new Dictionary<string, IDictionary<string, bool>>();
            filter.Add("name", new Dictionary<string, bool>(){{DENNETWORK_NAME, true}});

            IList<NetworkResponse> responses = await this.DockerClient.Networks.ListNetworksAsync
            (
                new NetworksListParameters()
                {
                    Filters = filter,
                }
            );

            List<string> networkIds = new List<string>();
            foreach(NetworkResponse response in responses)
            {
                networkIds.Add(response.ID);
            }

            return networkIds;
        }

        public async Task<bool> StartWaykDen()
        {
            List<string> networks = await this.GetDenNetwork();

            if(networks.Count == 0)
            {
                await this.DenNetwork.CreateNetwork();
            } else {
                this.DenNetwork.SetNetworkId(networks[0]);
            }

            if(string.IsNullOrEmpty(this.DenConfig.DenLucidConfigObject.ApiKey))
            {
                this.DenConfig.DenLucidConfigObject.ApiKey = DenServiceUtils.Generate(32);
            }

            if(string.IsNullOrEmpty(this.DenConfig.DenLucidConfigObject.AdminSecret))
            {
                this.DenConfig.DenLucidConfigObject.AdminSecret = DenServiceUtils.Generate(10);
            }

            if(string.IsNullOrEmpty(this.DenConfig.DenLucidConfigObject.AdminUsername))
            {
                this.DenConfig.DenLucidConfigObject.AdminUsername = DenServiceUtils.Generate(16);
            }

            if(string.IsNullOrEmpty(this.DenConfig.DenServerConfigObject.ApiKey))
            {
                this.DenConfig.DenServerConfigObject.ApiKey = DenServiceUtils.Generate(32);
            }

            if(string.IsNullOrEmpty(this.DenConfig.DenPickyConfigObject.ApiKey))
            {
                this.DenConfig.DenPickyConfigObject.ApiKey = DenServiceUtils.Generate(32);
            }

            bool started = await this.StartDenMongo();
            started = started ? await this.StartDenPicky(): false;
            started = started ? await this.StartDenLucid(): false;
            started = started ? await this.StartDenRouter(): false;
            started = started ? await this.StartDenServer(): false;
            started = started ? await this.StartTraefikService(): false;
            
            this.denConfigController.StoreConfig(this.DenConfig);
            return started;
        }

        public async Task StopWaykDen(List<string> containerIds)
        {
            if(this.OnLog != null)
                        this.OnLog($"Stopping WaykDen");

            foreach(string service in containerIds)
            {   
                bool stopped = await this.StopContainer(service);
                if(this.OnLog != null && stopped)
                {
                    this.ServicesCount--;
                    this.OnLog($"{service} stopped");
                }
            }

            if(Directory.Exists($"{this.Path}{System.IO.Path.DirectorySeparatorChar}traefik"))
            {
                Directory.Delete($"{this.Path}{System.IO.Path.DirectorySeparatorChar}traefik", true);
            }
        }

        public string CreateDockerCompose()
        {
            return ExportDenConfigUtils.CreateDockerCompose(this.GetDenServicesConfig());
        }

        private DenService[] GetDenServicesConfig()
        {
            return new DenService[]{new DenMongoService(this), new DenPickyService(this), new DenLucidService(this), new DenRouterService(this), new DenServerService(this), new DenTraefikService(this)};
        }
        public void WriteLog(string message)
        {
            this.OnLog(message);
        }

        public void WriteError(Exception e)
        {
            this.OnError(e);
        }

        public delegate void OnLogHandler(string message);
        public event OnLogHandler OnLog;

        public delegate void OnErrorHandler(Exception e);
        public event OnErrorHandler OnError;
    }
}