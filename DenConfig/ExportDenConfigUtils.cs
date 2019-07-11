using System.Collections.Generic;
using System.Text;
using WaykPS.Services.MicroServices;

namespace WaykPS.Config
{
    public class ExportDenConfigUtils
    {
        private const string DOCKER_COMPOSE_VERSION = "3.4";
        private static List<string> volumesString = new List<string>();
        public static string CreateDockerCompose(DenService[] services)
        {
            string dockercompose = 
$@"version: '{DOCKER_COMPOSE_VERSION}'

services:";
            StringBuilder sb = new StringBuilder();
            sb.Append(dockercompose);

            foreach(DenService service in services)
            {
                sb.AppendLine();
                sb.Append(CreateServiceDockerCompose(service));
            }

            sb.AppendLine();
            sb.AppendLine("volumes:");
            sb.AppendLine($"  den-mongodata:");

            sb.AppendLine();
            sb.AppendLine("networks:");
            sb.AppendLine("  den-network:");
            sb.AppendLine("    driver: bridge");

            return sb.ToString();
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
                case DenPickyService denPicky:
                    sb.AppendLine("    depends_on:");
                    sb.AppendLine("      - den-mongo");
                    break;

                case DenLucidService denLucid:
                    sb.AppendLine("    depends_on:");
                    sb.AppendLine("      - den-mongo");
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
                    string[] splittedEnv = env.Split('=', 2);

                    if(splittedEnv.Length < 2)
                    {
                        continue;
                    }

                    string envString = "      {0}: '{1}'";

                    if(splittedEnv[1].Contains('\n'))
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
                    string adjustcmd = command.Contains(' ') ? command.Insert(command.IndexOf('=') + 1, "\'").Insert(command.Length + 1, "\'") : command;
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
    }
}