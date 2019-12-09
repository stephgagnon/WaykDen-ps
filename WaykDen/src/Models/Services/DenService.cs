using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using WaykDen.Cmdlets;
using WaykDen.Controllers;

namespace WaykDen.Models.Services
{
    public class DenService : PSCmdlet
    {
        protected string Path => DenServicesController?.Path;
        public Dictionary<string, IList<PortBinding>> PortBindings = new Dictionary<string, IList<PortBinding>>();
        public Dictionary<string, EmptyStruct> ExposedPorts = new Dictionary<string, EmptyStruct>();
        public List<string> Env = new List<string>();
        public List<string> Cmd = new List<string>();
        public List<string> Volumes = new List<string>();
        protected LogConfig LogConfig = new LogConfig();
        protected List<string> HealthCheck = new List<string>();
        public string Name = string.Empty;
        protected string Container_ID = string.Empty;
        public string ImageName = string.Empty;
        protected DenServicesController DenServicesController;
        protected DenConfig DenConfig => DenServicesController?.DenConfig;
        protected DockerClient DockerClient => DenServicesController?.DockerClient;
        public DenService()
        {
        }

        public DenService(DenServicesController controller, string name)
        {
            this.Name = name;
            this.DenServicesController = controller;

            if(!string.IsNullOrEmpty(this.DenConfig.DenDockerConfigObject.SyslogServer))
            {
                this.LogConfig  = new LogConfig()
                {
                    Type = "syslog",
                    Config = new Dictionary<string, string>()
                    {
                        {"syslog-address", $"{this.DenConfig.DenDockerConfigObject.SyslogServer}"},
                        {"syslog-facility", "daemon"},
                        {"syslog-format", "rfc5424"},
                        {"tag", this.Name}
                    }
                };
            }
        }

        public virtual async Task<CreateContainerResponse> CreateContainer(string image)
        {
            LogConfig logConfig = new LogConfig();
            
            for(int i = 0; i < this.Volumes.Count; i++)
            {
                this.Volumes[i] = $"{this.Path}{System.IO.Path.DirectorySeparatorChar}{this.Volumes[i]}";
            }

            HealthConfig healthConfig = null;

            if(this.HealthCheck.Count > 0)
            {
                healthConfig = new HealthConfig()
                {
                    Interval = TimeSpan.FromSeconds(5),
                    Retries = 5,
                    //Bad JSON conversion for Timeout, value need to be in nanosecond.
                    Timeout = TimeSpan.FromSeconds(2 * 1000000000),
                    Test = this.HealthCheck,
                };
            }

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
                        Healthcheck = healthConfig
                    }
                )
                {
                    Name = this.Name,
                    HostConfig = new HostConfig()
                    {
                        PortBindings = this.PortBindings,
                        AutoRemove = false,
                        Binds = this.Volumes,
                        LogConfig = this.LogConfig,
                        Isolation = this.DenConfig.DenDockerConfigObject.Platform == Platforms.Windows.ToString() ? "process" : string.Empty,
                    },
                    NetworkingConfig = new NetworkingConfig()
                    {
                        EndpointsConfig = new Dictionary<string, EndpointSettings>
                        {
                            {"den-network", new EndpointSettings()
                                {
                                    NetworkID = this.DenServicesController.DenNetwork.NetworkId
                                }
                            }
                        }
                    }
                }
            );
        }

        public virtual async Task<bool> StartContainer(string id)
        {
            try
            {
                this.Container_ID = id;

                return await this.DockerClient.Containers.StartContainerAsync(
                    this.Container_ID,
                    new ContainerStartParameters()
                );
            }
            catch(Exception e)
            {
                this.DenServicesController.WriteError(e);
            }

            return false;
        }

        public virtual async Task CreateImage()
        {
            try
            {
                string[] repo = this.ImageName.Split(':');
                if(repo.Length > 1)
                {


                    await this.DockerClient.Images.CreateImageAsync
                    (
                        new ImagesCreateParameters
                        {
                            Repo = repo[0],
                            Tag = repo[1],
                            FromImage = this.ImageName
                        },
                        null,
                        new Progress(this.DenServicesController)
                    );
                }
            }
            catch(Exception e)
            {
                if (this is DenMongoService && this.DenConfig.DenDockerConfigObject.Platform == "Linux")
                {
                    this.DenServicesController.WriteError(e);
                }
            }
        }

        public MultiplexedStream Stream;

        protected virtual void AddPortBinding(string port, List<PortBinding> binding)
        {
            this.PortBindings.Add(port, binding);
        }

        public async Task<string> GetServiceLogs()
        {
            Stream logs = await this.DockerClient.Containers.GetContainerLogsAsync
            (
                this.Container_ID,
                new ContainerLogsParameters()
                {
                    ShowStdout = true
                }
            );

            StreamReader reader = new StreamReader(logs);
            return reader.ReadToEnd();
        }

        public async Task<bool> CheckVolumeExist()
        {
            VolumesListResponse vlr = await this.DockerClient.Volumes.ListAsync();
            VolumeResponse vr = vlr.Volumes.Where(x => x.Name == $"{this.Name}data").FirstOrDefault();
            if(vr != null)
            {
                return true;
            }

            return false;
        }

        public async Task<VolumeResponse> CreateVolume()
        {
            VolumeResponse vr = await this.DockerClient.Volumes.CreateAsync
            (
                new VolumesCreateParameters
                {
                    Name = $"{this.Name}data",
                }
            );

            return vr;
        }

        public bool GetVolumes(out List<string> volumes)
        {
            volumes = this.Volumes;
            if(this.Volumes.Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool GetEnvironment(out List<string> envs)
        {
            envs = this.Env;
            if(this.Env.Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool GetCmd(out List<string> cmds)
        {
            cmds = this.Cmd;

            if(this.Cmd.Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool GetPorts(out List<string> ports)
        {
            List<string> p = new List<string>();
            ports = p;
            if(this.PortBindings.Count > 0)
            {
                this.PortBindings.ToList().ForEach(delegate(KeyValuePair<string, IList<PortBinding>> value){
                    p.Add($"{value.Key}:{value.Value.First().HostPort}");
                });

                return true;
            }

            return false;
        }

        public bool GetLogConfigs(out Dictionary<string, string> logConfigs)
        {
            logConfigs  = this.LogConfig.Config as Dictionary<string, string>;
            if(this.LogConfig.Type != null && this.LogConfig.Type.Equals("syslog"))
            {
                if(!logConfigs.ContainsKey("driver"))
                {
                    logConfigs.Add("driver", this.LogConfig.Type);
                }
                return true;
            }

            return false;
        }

        public async virtual Task<bool> IsHealthy()
        {
            return await this.IsRunning();
        }

        protected async virtual Task<bool> IsRunning()
        {
            ContainerInspectResponse cir = await this.DockerClient.Containers.InspectContainerAsync(this.Container_ID);
            return cir.State.Running;
        }

        private class Progress : IProgress<JSONMessage>
        {
            private DenServicesController denServiceController;
            public Progress(DenServicesController controller)
            {
                this.denServiceController = controller;
            }
            public void Report(JSONMessage value)
            {
                this.denServiceController.WriteLog(value.Status);
            }
        }
    }
}