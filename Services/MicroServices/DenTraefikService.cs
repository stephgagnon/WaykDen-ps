using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using WaykPS.Controllers;

namespace WaykPS.Services.MicroServices
{
    public class DenTraefikService : DenService
    {
        private const string TRAEFIK_NAME = "traefik";
        private const string TRAEFIK_REST_CMD = "--rest";
        private const string TRAEFIK_API_CMD = "--api";
        private const string TRAEFIK_LOGLEVEL_CMD = "--loglevel=INFO";
        private const string TRAEFIK_ACCESS_LOG_CMD = "--accesslog.filepath=access.log";
        private string traefikEntrypoints = string.Empty;
        private string entrypoints = "ws";
        public DenTraefikService(DenServicesController controller)
        {
            this.DenServicesController = controller;
            this.Name = TRAEFIK_NAME;
            this.ImageName = this.DenConfig.DenImageConfigObject.DenTraefikImage;
            this.ExposedPorts.Add(this.DenConfig.DenTraefikConfigObject.WaykDenPort, new EmptyStruct());
            this.ExposedPorts.Add(this.DenConfig.DenTraefikConfigObject.ApiPort, new EmptyStruct());
            this.PortBindings.Add(this.DenConfig.DenTraefikConfigObject.WaykDenPort, new List<PortBinding>(){new PortBinding(){HostIP = "0.0.0.0", HostPort = this.DenConfig.DenTraefikConfigObject.WaykDenPort}});
            this.PortBindings.Add(this.DenConfig.DenTraefikConfigObject.ApiPort, new List<PortBinding>(){new PortBinding(){HostIP = "0.0.0.0", HostPort = this.DenConfig.DenTraefikConfigObject.ApiPort}});
            if(!string.IsNullOrEmpty(this.DenConfig.DenTraefikConfigObject.Certificate))
            {
              this.ImportCertificate();
              this.traefikEntrypoints = $"--entrypoints=Name:https Address::{this.DenConfig.DenTraefikConfigObject.WaykDenPort} TLS:/data/traefik.cert,/data/traefik.key";
              this.entrypoints = "https";
            }
            else
            {
              this.traefikEntrypoints = $"--entrypoints=Name:ws Address::{this.DenConfig.DenTraefikConfigObject.WaykDenPort}";
            }

            this.Cmd.Add(TRAEFIK_REST_CMD);
            this.Cmd.Add(TRAEFIK_API_CMD);
            this.Cmd.Add(this.traefikEntrypoints);
            this.Cmd.Add(TRAEFIK_ACCESS_LOG_CMD);
            this.Cmd.Add(TRAEFIK_LOGLEVEL_CMD);
        }

        private string BuildTraefikToml(string externalUrl)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.BuildConfig());
            return sb.ToString();
        }

        private string BuildConfig()
        {
            string traefikConfig = 
$@"
{{
  ""frontends"": {{
    ""lucid"": {{
      ""backend"": ""lucid"",
      ""passHostHeader"": true,
      ""entrypoints"": [
        ""{this.entrypoints}""
      ],
      ""routes"": {{
        ""lucid"": {{
          ""rule"": ""PathPrefixStripRegex:/lucid""
        }}
      }}
    }},
    ""lucidop"": {{
      ""backend"": ""lucid"",
      ""passHostHeader"": true,
      ""entrypoints"": [
        ""{this.entrypoints}""
      ],
      ""routes"": {{
        ""lucidop"": {{
          ""rule"": ""PathPrefix:/op""
        }}
      }}
    }},
    ""lucidauth"": {{
      ""backend"": ""lucid"",
      ""passHostHeader"": true,
      ""entrypoints"": [
        ""{this.entrypoints}""
      ],
      ""routes"": {{
        ""lucidauth"": {{
          ""rule"": ""PathPrefix:/auth""
        }}
      }}
    }},
    ""router"": {{
      ""backend"": ""router"",
      ""passHostHeader"": true,
      ""entrypoints"": [
        ""{this.entrypoints}""
      ],
      ""routes"": {{
        ""router"": {{
          ""rule"": ""PathPrefixStripRegex:/cow""
        }}
      }}
    }},""server"": {{
      ""backend"": ""server"",
      ""passHostHeader"": true,
      ""entrypoints"": [
        ""{this.entrypoints}""
      ]
    }}
  }},
  ""backends"": {{
    ""lucid"": {{
      ""servers"": {{
        ""lucid"": {{
          ""url"": ""http://den-lucid:4242""
        }}
      }}
    }},
    ""router"": {{
      ""servers"": {{
        ""router"": {{
	        ""url"": ""http://den-router:4491""
        }}
      }}
    }},
    ""server"": {{
      ""servers"": {{
        ""server"": {{
	        ""url"": ""http://den-server:10255""
        }}
      }}
    }}
  }}
}}
";
            return traefikConfig;
        }

        public async Task<bool> CurlTraefikConfig()
        {
            using(var httpClient = new HttpClient())
            {
                string port =  this.DenConfig.DenTraefikConfigObject.ApiPort;
                using (var request = new HttpRequestMessage(new HttpMethod("PUT"), $"http://127.0.0.1:{port}/api/providers/rest"))
                {
                    request.Content = new StringContent(this.BuildConfig(), Encoding.UTF8);
                    var response = httpClient.SendAsync(request);
                    response.Wait();
                    return (await response).IsSuccessStatusCode;
                }
            }
        }

        private void ImportCertificate()
        {
          this.DenServicesController.Path.TrimEnd(System.IO.Path.DirectorySeparatorChar);
          string path = $"{this.DenServicesController.Path}{System.IO.Path.DirectorySeparatorChar}traefik";
          if(Directory.Exists(path))
          {
            Directory.Delete(path, true);
          }

          Directory.CreateDirectory(path);
          File.WriteAllText($"{path}{System.IO.Path.DirectorySeparatorChar}traefik.cert", this.DenConfig.DenTraefikConfigObject.Certificate);
          File.WriteAllText($"{path}{System.IO.Path.DirectorySeparatorChar}traefik.key", this.DenConfig.DenTraefikConfigObject.PrivateKey);
          this.Volumes.Add($"{path}:/data");
        }
    }
}