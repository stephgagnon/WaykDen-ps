using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using WaykDen.Utils;
using WaykDen.Models;
using WaykDen.Models.Services;

namespace WaykDen.Controllers
{
    public class DenServicesController
    {
        private const string WAYK_DEN_HOME = "WAYK_DEN_HOME";

        private const string DEN_NETWORK_NAME = "den-network";

        public string Path {get; set;} = string.Empty;

        public DenConfig DenConfig {get;}

        public DenNetwork DenNetwork {get;}

        public DockerClient DockerClient {get;}

        public List<DenService> RunningDenServices = new List<DenService>();

        public int ServicesCount;

        private DenConfigController denConfigController;

        private Dictionary<Container, string> ContainersName = new Dictionary<Container, string>()
        {
            {Container.DenMongo, "den-mongo"},
            {Container.DenPicky, "den-picky"},
            {Container.DenLucid, "den-lucid"},
            {Container.DenServer, "den-server"},
            {Container.Traefik, "traefik"}
        };

        private enum Container
        {
            DenMongo,
            DenPicky,
            DenLucid,
            DenServer,
            Traefik,
            DevolutionsJet
        }

        private enum ContainerState
        {
            Created = 0x0001,
            Running = 0x0010,
            Exited = 0x0100
        }

        private enum ContainerFilter
        {
            Name,
            Network
        }

        public DenServicesController(DenConfigController denConfigController)
        {
            this.denConfigController = denConfigController;   
        }

        public DenServicesController(string path, DenConfigController denConfigController = null) : this(denConfigController)
        {
            this.Path = Environment.GetEnvironmentVariable(WAYK_DEN_HOME);
            if(string.IsNullOrEmpty(this.Path))
            {
                this.Path = path;
            }

            this.Path = this.Path.EndsWith($"{System.IO.Path.DirectorySeparatorChar}") ? this.Path : $"{this.Path}{System.IO.Path.DirectorySeparatorChar}";
            this.DenConfig = this.denConfigController.GetConfig();
            this.DenNetwork = new DenNetwork(this);

            if(this.DenConfig == null)
            {
                throw new Exception("Could not find WaykDen configuration in given path. Make sure WaykDen configuration is in current folder or set WAYK_DEN_HOME to the path of WaykDen configuration");
            }

            Platforms p = string.Equals(this.DenConfig.DenDockerConfigObject.Platform, "Linux") ? Platforms.Linux : Platforms.Windows;
            this.DenConfig.DenImageConfigObject = new DenImageConfigObject(p);
                            
            this.DockerClient = new DockerClientConfiguration(new Uri(this.DenConfig.DenDockerConfigObject.DockerClientUri)).CreateClient();
        }

        private async Task<bool> StartDenMongo()
        {
            DenMongoService mongo = new DenMongoService(this);
            if(mongo.IsExternal)
            {
                return true;
            }
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

        private async Task<bool> StartDenServer(bool multipleInstance = false, int instanceId = 1)
        {
            DenServerService server = new DenServerService(this, multipleInstance, instanceId);
            return await this.StartService(server);
        }

        private async Task<bool> StartTraefikService(int instanceCount = 1)
        {
            DenTraefikService traefik = new DenTraefikService(this, instanceCount);
            return await this.StartService(traefik);
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

                if(!started)
                {
                    if(this.OnError != null) this.OnError(new Exception($"Error running {service.Name} container"));
                } 
                else
                {
                    if(this.OnLog != null) this.OnLog($"{service.Name} started");
                    
                    if(service is DenHealthCheckService denHealthCheckService)
                    {
                        if(this.OnLog != null) this.OnLog($"Waiting for {service.Name} health status");

                        started = await service.IsHealthy();

                        if(started)
                        {
                            if(this.OnLog != null) this.OnLog($"{service.Name} is healthy");
                        }
                    }

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

        public async Task<List<string>> GetRunningContainers()
        {
            return await this.ListContainers(ContainerState.Running, ContainerFilter.Network, DEN_NETWORK_NAME);
        }

        public async Task RemoveWaykDenContainers()
        {
            this.ContainersName.TryGetValue(Container.DenMongo, out var mongo);
            this.ContainersName.TryGetValue(Container.DenPicky, out var picky);
            this.ContainersName.TryGetValue(Container.DenLucid, out var lucid);
            this.ContainersName.TryGetValue(Container.DenServer, out var server);
            this.ContainersName.TryGetValue(Container.Traefik, out var traefik);
            this.ContainersName.TryGetValue(Container.DevolutionsJet, out var jet);

            string[] containersName = {
                mongo,
                picky,
                lucid,
                server,
                traefik
            };
            
            foreach(string name in containersName)
            {
                List<string> id = await this.ListContainers(ContainerState.Exited | ContainerState.Created, ContainerFilter.Name, name);
                if(id.Count > 0)
                {
                    this.WriteLog($"Removing {name}");
                    await this.DockerClient.Containers.RemoveContainerAsync
                    (
                        id[0],
                        new ContainerRemoveParameters(){}
                    );
                }
            }
        }

        private async Task<List<string>> ListContainers(ContainerState containerState, ContainerFilter containerFilter, string param)
        {
            if(this.DockerClient == null)
            {
                return null;
            }

            IDictionary<string, IDictionary<string, bool>> filter = new Dictionary<string, IDictionary<string, bool>>();

            if((containerState & ContainerState.Exited) == ContainerState.Exited)
            {
                filter.Add("status", new Dictionary<string, bool>(){{"exited", true}});
                filter.Add("name", new Dictionary<string, bool>(){{param, true}});
            }
            
            if((containerState & ContainerState.Created) == ContainerState.Created)
            {
                bool ok = filter.TryGetValue("status", out IDictionary<string, bool> f);
                if(ok)
                {
                    f.Add("created", true);
                }
                else
                {
                    filter.Add("status", new Dictionary<string, bool>(){{"created", true}});
                    filter.Add("name", new Dictionary<string, bool>(){{param, true}});   
                }
            }
            else
            {
                if(containerFilter == ContainerFilter.Name)
                {
                    filter.Add("name", new Dictionary<string, bool>(){{param, true}});
                } 
                else if(containerFilter == ContainerFilter.Network)
                {
                    filter.Add("network", new Dictionary<string, bool>(){{param, true}});
                }
            }

            Task<IList<ContainerListResponse>> responsesTask = this.DockerClient.Containers.ListContainersAsync
            (
                new ContainersListParameters(){Filters = filter}
            );

            responsesTask.Wait();
            IList<ContainerListResponse> responses = await responsesTask;

            List<string> containerIds = new List<string>();

            foreach(ContainerListResponse response in responses)
            {
                containerIds.Add(response.ID);
            }

            return containerIds;
        }

        public async Task<List<string>> GetDenNetwork()
        {
            if(this.DockerClient == null)
            {
                return null;
            }

            IDictionary<string, IDictionary<string, bool>> filter = new Dictionary<string, IDictionary<string, bool>>();
            filter.Add("name", new Dictionary<string, bool>(){{DEN_NETWORK_NAME, true}});

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

        public async Task<bool> StartWaykDen(int instanceCount = 1)
        {
            try
            {
                Task t = this.RemoveWaykDenContainers();
                t.Wait();

                List<string> networks = await this.GetDenNetwork();

                if(networks.Count == 0)
                {
                    await this.DenNetwork.CreateNetwork();
                } else {
                    this.DenNetwork.SetNetworkId(networks[0]);
                }

                if(string.IsNullOrEmpty(this.DenConfig.DenLucidConfigObject.ApiKey))
                {
                    this.DenConfig.DenLucidConfigObject.ApiKey = DenServiceUtils.GenerateRandom(32);
                }

                if(string.IsNullOrEmpty(this.DenConfig.DenLucidConfigObject.AdminSecret))
                {
                    this.DenConfig.DenLucidConfigObject.AdminSecret = DenServiceUtils.GenerateRandom(10);
                }

                if(string.IsNullOrEmpty(this.DenConfig.DenLucidConfigObject.AdminUsername))
                {
                    this.DenConfig.DenLucidConfigObject.AdminUsername = DenServiceUtils.GenerateRandom(16);
                }

                if(string.IsNullOrEmpty(this.DenConfig.DenServerConfigObject.ApiKey))
                {
                    this.DenConfig.DenServerConfigObject.ApiKey = DenServiceUtils.GenerateRandom(32);
                }

                if(string.IsNullOrEmpty(this.DenConfig.DenPickyConfigObject.ApiKey))
                {
                    this.DenConfig.DenPickyConfigObject.ApiKey = DenServiceUtils.GenerateRandom(32);
                }

                if(!string.IsNullOrEmpty(this.DenConfig.DenTraefikConfigObject.Certificate) && (string.IsNullOrEmpty(this.DenConfig.DenTraefikConfigObject.PrivateKey)))
                {
                    this.WriteError(new Exception("No private key found for certificate. Add private key or remove certificate"));
                    return false;
                }

                bool started = await this.StartDenMongo();
                started = started ? await this.StartDenPicky(): false;
                started = started ? await this.StartDenLucid() : false;

                int count = 1;
                while (count != instanceCount + 1)
                {
                    started = started ? await this.StartDenServer(instanceCount > 1, count) : false;
                    count++;
                }

                if (started)
                {
                    Task<bool> traefikStarted = this.StartTraefikService(instanceCount);
                    traefikStarted.Wait();
                    started = await traefikStarted;
                    if(!started)
                    {
                        this.WriteError(new Exception("Error starting Traefik service. Make sure External URL is well configured."));
                    }
                }

                return started;
            }
            catch(Exception e)
            {
                this.WriteError(e);
                return false;
            }
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

        public string[] CreateDockerCompose()
        {
            Platforms platform = this.DenConfig.DenDockerConfigObject.Platform == "Linux" ? Platforms.Linux : Platforms.Windows;
            return ExportDenConfigUtils.CreateDockerCompose(this.GetDenServicesConfig(), platform);
        }

        public string CreateTraefikToml()
        {
            return ExportDenConfigUtils.CreateTraefikToml(new DenTraefikService(this));
        }

        public string CreateScript(string exportPath, bool podman, Platforms platform)
        {
            this.Path = exportPath;
            string config = this.DenConfig.ConvertToPwshParameters(platform);
            return ExportDenConfigUtils.CreateScript(podman, this.GetDenServicesConfig(), exportPath, config);
        }

        private DenService[] GetDenServicesConfig()
        {
            return new DenService[]{new DenMongoService(this), new DenPickyService(this), new DenLucidService(this), new DenServerService(this), new DenTraefikService(this)};
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