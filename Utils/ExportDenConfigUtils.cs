using System;
using System.Collections.Generic;
using System.Text;
using WaykDen.Models.Services;
using WaykDen.Models;

namespace WaykDen.Utils
{
    public class ExportDenConfigUtils
    {
        private const string DOCKER_COMPOSE_VERSION = "3.4";
        private static List<string> volumesString = new List<string>();
        public static string[] CreateDockerCompose(DenService[] services, Platforms platform)
        {
            string traefiktoml = string.Empty;
            string dockercompose = 
$@"version: '{DOCKER_COMPOSE_VERSION}'

services:";
            StringBuilder sb = new StringBuilder();
            sb.Append(dockercompose);

            foreach(DenService service in services)
            {
                if(service is DenMongoService mongo && mongo.IsExternal)
                {
                    continue;
                }

                if(service is DenTraefikService traefik)
                {
                    traefiktoml = CreateTraefikToml(traefik);
                }

                sb.AppendLine();
                sb.Append(CreateServiceDockerCompose(service));
            }

            sb.AppendLine();
            sb.AppendLine("volumes:");
            sb.AppendLine($"  den-mongodata:");

            sb.AppendLine();
            sb.AppendLine("networks:");
            sb.AppendLine("  den-network:");
            string networkDriver = platform == Platforms.Linux ? "bridge" : "nat";
            sb.AppendLine($"    driver: {networkDriver}");

            return new string[]{sb.ToString(), traefiktoml};
        }

        private static string CreateServiceDockerCompose(DenService service)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("  {0}:", service.Name));
            sb.AppendLine(string.Format("    image: '{0}'", service.ImageName));
            sb.AppendLine(string.Format("    container_name: '{0}'", service.Name));
            string[] dependency = null;
            string[] dependencyPort = null;
            switch(service)
            {
                case DenLucidService denLucid:
                    sb.AppendLine("    depends_on:");
                    sb.AppendLine("      - den-picky");
                    break;
                
                case DenRouterService denRouter:
                    sb.AppendLine("    depends_on:");
                    sb.AppendLine("        - den-lucid");
                    break;

                case DenServerService denServer:
                    dependency = new string[]{"den-router", "den-lucid"};
                    dependencyPort = new string[]{"10254/healtz", "4242/health"};
                    sb.AppendLine("    depends_on:");
                    sb.AppendLine("        - traefik");
                    break;

                case DenTraefikService denTraefik:
                    sb.AppendLine("    depends_on:");
                    sb.AppendLine("        - den-router");
                    break;
            }

            if(dependency != null && dependencyPort != null && dependency.Length == dependencyPort.Length)
            {
                sb.AppendLine("    healthcheck:");
                string curlString = "      test: {0}";
                string d = string.Empty;
                for(int i = 0; i < dependency.Length; i++)
                {
                    string join = i > 0 ? " && " : string.Empty;
                    d += $"{join}curl -sS http://{dependency[i]}:{dependencyPort[i]}";
                }
                sb.AppendLine(string.Format(curlString, d));
                sb.AppendLine("      interval: 30s");
                sb.AppendLine("      timeout: 10s");
                sb.AppendLine("      retries: 5");
                sb.AppendLine("      start_period: 40s");
            }

            sb.AppendLine("    networks:");
            sb.AppendLine("      den-network:");
            
            if(service.GetEnvironment(out List<string> envs))
            {
                sb.AppendLine("    environment:");
                foreach(string env in envs)
                {
                    string[] splittedEnv = env.Split(new char[]{'='}, 2);

                    if(splittedEnv.Length < 2)
                    {
                        continue;
                    }

                    string envString = "      {0}: '{1}'";

                    if(splittedEnv[1].Contains("\n"))
                    {
                        string envKeyString = "      {0}: |";
                        sb.AppendLine(string.Format(envKeyString, splittedEnv[0]));

                        foreach(string s in splittedEnv[1].Split('\n'))
                        {
                            sb.AppendLine(string.Format("        {0}", s));
                        }
                    }
                    else
                    {
                        sb.AppendLine(string.Format(envString, splittedEnv[0], splittedEnv[1]));
                    }
                }
            }

            if(service.GetVolumes(out List<string> volumes))
            {
                sb.AppendLine("    volumes:");

                string volumeString = "       - '{0}'";

                foreach(string volume in volumes)
                {
                    sb.AppendLine(string.Format(volumeString, volume));
                }
            }

            if(service.GetCmd(out List<string> commands))
            {
                sb.Append("    command:");

                foreach(string command in commands)
                {
                    string adjustcmd = command.Contains(" ") ? command.Insert(command.IndexOf('=') + 1, "\'").Insert(command.Length + 1, "\'") : command;
                    sb.Append($" {adjustcmd}");
                }

                sb.AppendLine();
            }

            if(service.GetPorts(out List<string> ports))
            {
                sb.AppendLine("    ports:");

                foreach(string port in ports)
                {
                    sb.AppendLine(string.Format("      - \"{0}\"", port));
                }
            }

            return sb.ToString();
        }

        public static string CreateTraefikToml(DenTraefikService traefik)
        {
            string toml =
@"logLevel = ""INFO""

[file]

[entryPoints]
    [EntryPoints.{0}]
    address = "":{1}""
    {2}

[frontends]
    [frontends.lucid]
    passHostHeader = true
    backend = ""lucid""
    entrypoints = [""{0}""]
        [frontends.lucid.routes.lucid]
        rule = ""PathPrefixStrip:/lucid""

    [frontends.lucidop]
    passHostHeader = true
    backend = ""lucid""
    entrypoints = [""{0}""]
        [frontends.lucidop.routes.lucidop]
        rule = ""PathPrefix:/op""

    [frontends.lucidauth]
    passHostHeader = true
    backend = ""lucid""
    entrypoints = [""{0}""]
        [frontends.lucidauth.routes.lucidauth]
        rule = ""PathPrefix:/auth""

    [frontends.router]
    passHostHeader = true
    backend = ""router""
    entrypoints = [""{0}""]
        [frontends.router.routes.router]
        rule = ""PathPrefixStrip:/cow""

    [frontends.server]
    passHostHeader = true
    backend = ""server""
    entrypoints = [""{0}""]

[backends]
    [backends.lucid]
        [backends.lucid.servers.lucid]
        url = ""http://den-lucid:4242""
        weight = 10

    [backends.router]
        [backends.router.servers.router]
        url = ""http://den-router:4491""
        weight = 10

    [backends.server]
        [backends.server.servers.server]
        url = ""http://den-server:10255""
        weight = 10
";

            return string.Format(toml, traefik.Entrypoints, traefik.WaykDenPort, traefik.Tls);
        }
    }
}