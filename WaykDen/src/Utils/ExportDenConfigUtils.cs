using System;
using System.Collections.Generic;
using System.Text;
using WaykDen.Models.Services;
using WaykDen.Models;
using System.Management.Automation;

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

            foreach (DenService service in services)
            {
                if (service is DenMongoService mongo && mongo.IsExternal)
                {
                    continue;
                }

                if (service is DenTraefikService traefik)
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

            return new string[] { sb.ToString(), traefiktoml };
        }

        private static string CreateServiceDockerCompose(DenService service)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("  {0}:", service.Name));
            sb.AppendLine(string.Format("    image: '{0}'", service.ImageName));
            sb.AppendLine(string.Format("    container_name: '{0}'", service.Name));
            string[] dependency = null;
            string[] dependencyPort = null;
            switch (service)
            {
                case DenLucidService denLucid:
                    sb.AppendLine("    depends_on:");
                    sb.AppendLine("      - den-picky");
                    break;

                case DenServerService denServer:
                    dependency = new string[] { "den-lucid" };
                    dependencyPort = new string[] { "10254/healtz", "4242/health" };
                    sb.AppendLine("    depends_on:");
                    sb.AppendLine("        - traefik");
                    break;

                case DenTraefikService denTraefik:
                    break;
            }

            if (dependency != null && dependencyPort != null && dependency.Length == dependencyPort.Length)
            {
                sb.AppendLine("    healthcheck:");
                string curlString = "      test: {0}";
                string d = string.Empty;
                for (int i = 0; i < dependency.Length; i++)
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

            if (service.GetEnvironment(out List<string> envs))
            {
                sb.AppendLine("    environment:");
                foreach (string env in envs)
                {
                    string[] splittedEnv = env.Split(new char[] { '=' }, 2);

                    if (splittedEnv.Length < 2)
                    {
                        continue;
                    }

                    string envString = "      {0}: '{1}'";

                    if (splittedEnv[1].Contains("\n"))
                    {
                        string envKeyString = "      {0}: |";
                        sb.AppendLine(string.Format(envKeyString, splittedEnv[0]));

                        foreach (string s in splittedEnv[1].Split('\n'))
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

            if (service.GetVolumes(out List<string> volumes))
            {
                sb.AppendLine("    volumes:");

                string volumeString = "       - '{0}'";

                foreach (string volume in volumes)
                {
                    sb.AppendLine(string.Format(volumeString, volume));
                }
            }

            if (service.GetCmd(out List<string> commands))
            {
                sb.Append("    command:");

                foreach (string command in commands)
                {
                    string adjustcmd = command.Contains(" ") ? command.Insert(command.IndexOf('=') + 1, "\'").Insert(command.Length + 1, "\'") : command;
                    sb.Append($" {adjustcmd}");
                }

                sb.AppendLine();
            }

            if (service.GetPorts(out List<string> ports))
            {
                sb.AppendLine("    ports:");

                foreach (string port in ports)
                {
                    sb.AppendLine(string.Format("      - \"{0}\"", port));
                }
            }

            if (service.GetLogConfigs(out Dictionary<string, string> logConfigs))
            {
                if (logConfigs.TryGetValue("driver", out string driver))
                {
                    sb.AppendLine("    logging:");
                    sb.AppendLine($"      driver: \"{driver}\"");
                }

                string logOptionsFormat = "        {0}: {1}";
                sb.AppendLine($"      options:");

                if (logConfigs.TryGetValue("syslog-address", out string address))
                {
                    sb.AppendLine(string.Format(logOptionsFormat, "syslog-address", address));
                }

                if (logConfigs.TryGetValue("syslog-facility", out string facility))
                {
                    sb.AppendLine(string.Format(logOptionsFormat, "syslog-facility", facility));
                }

                if (logConfigs.TryGetValue("syslog-format", out string format))
                {
                    sb.AppendLine(string.Format(logOptionsFormat, "syslog-format", format));
                }

                if (logConfigs.TryGetValue("tag", out string tag))
                {
                    sb.AppendLine(string.Format(logOptionsFormat, "tag", tag));
                }
            }

            return sb.ToString();
        }

        public static string CreateTraefikToml(DenTraefikService traefik, int instanceCount = 1)
        {
            string toml =
@"logLevel = ""INFO""

[file]

[entryPoints]
    [entryPoints.{0}]
    address = "":{1}""
        {2}
        [entryPoints.{0}.redirect]
	    regex = ""^http(s)?://([^/]+)/?$""
	    replacement = ""http$1://$2/web""

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
        [backends.router.servers.mainrouter]
        url = ""http://den-server:4491""
        method=""drr""
        weight = 10
";
        string serverService = string.Empty;

            if (instanceCount > 1)
            {
                string routeurService = string.Empty;
                while (instanceCount != 0)
                {
                    if (instanceCount == 1)
                    {
                        instanceCount--;
                        continue;
                    }

                    using (PowerShell PowerShellInstance = PowerShell.Create())
                    {
                        string denNumber = "_" + instanceCount;

                        PowerShellInstance.AddScript("docker inspect --format='{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' den-server" + denNumber);
                        var result = PowerShellInstance.Invoke();

                        string DenIP = string.Empty;
                        foreach (var item in result)
                        {
                            DenIP = item.ToString();
                        }

                        if (!string.IsNullOrEmpty(DenIP))
                        {
                            routeurService +=
$@"
        [backends.router.servers.router{instanceCount}]
        url = ""http://{DenIP}:4491""
        method=""drr""
        weight = 10
";
                            serverService +=
$@"
        [backends.server.servers.server{instanceCount}]
        url = ""http://{DenIP}:10255""
        method=""drr""
        weight = 10
";
                        }
                    }
                    instanceCount--;
                }

                toml += routeurService;
            }

        toml +=
@"
    [backends.server]
        [backends.server.servers.server]
        url = ""http://den-server:10255""
        weight = 10
        method=""drr""
" + serverService;

            return string.Format(toml, traefik.Entrypoints, traefik.WaykDenPort, traefik.Tls);
        }

        public static string CreateScript(bool podman, DenService[] services, string exportPath, string config)
        {
            string engine = podman ? "podman" : "docker";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"param(
[string] $Action
)

if([string]::IsNullOrEmpty($Action)){
    $Action = 'Start'
}");
            sb.AppendLine(config);

            List<string> servicesName = new List<string>();
            List<string> servicesCmd = new List<string>();
            List<string> servicesImage = new List<string>();
            foreach (DenService service in services)
            {
                if (service is DenMongoService denMongo)
                {
                    if (denMongo.IsExternal)
                    {
                        continue;
                    }
                }

                servicesName.Add(service.Name);
                servicesCmd.Add(service.Name.Replace("-", ""));
                servicesImage.Add(service.ImageName);
            }

            sb.AppendLine("$currentPath = Get-Location");
            sb.AppendLine($"$servicesName = @(\'{string.Join("\',\'", servicesName)}\')");
            if (podman)
            {
                sb.AppendLine($"$servicesImage = @(\"docker.io/{string.Join("\",\"docker.io/", servicesImage)}\")");
            } else sb.AppendLine($"$servicesImage = @(\"{string.Join("\",\"", servicesImage)}\")");
            sb.AppendLine($"$baseRun = \"{GetBaseDockerRunCmd(podman)}\"");
            List<string> health = new List<string>();
            if (podman)
            {
                health.Add("--healthcheck-interval=5s");
                health.Add("--healthcheck-timeout=2s");
                health.Add("--healthcheck-retries=5");
                health.Add("--healthcheck-start-period=1s");
            }
            else
            {
                health.Add("--health-interval=5s");
                health.Add("--health-timeout=2s");
                health.Add("--health-retries=5");
                health.Add("--health-start-period=1s");
            }
            sb.AppendLine($"$baseHealthCheck  = @(\'{string.Join("\',\'", health)}\')");

            if (podman)
            {
                sb.AppendLine("$servicesIP = 2..7 | ForEach-Object {\"10.88.123.$_\"}");
            }

            foreach (DenService service in services)
            {
                sb.AppendLine();
                sb.Append(GetServiceArgs(podman, service));
            }

            sb.AppendLine($"$commands = @(\'{string.Join("\',\'", servicesCmd)}\')");
            string createNetworkString = podman ? string.Empty : CreateNetworkFunction;
            string ipFunction = podman ? IPFunction : string.Empty;
            string createNetwork = podman ? string.Empty : "CreateNetwork";
            string healthCheck = podman ? "Healthcheck" : "Health";
            sb.AppendLine(string.Format(script, engine, createNetworkString, ipFunction, createNetwork, healthCheck));
            sb.AppendLine("waykden($Action)");
            return sb.ToString();
        }

        private static string GetBaseDockerRunCmd(bool podman)
        {
            List<string> baseCmd = new List<string>();
            if (podman)
            {
                baseCmd.AddRange(new string[] { "podman run --privileged -d" });
            }
            else baseCmd.AddRange(new string[] { "docker", "run", "-d", "--network=den-network" });
            return string.Join(" ", baseCmd);
        }

        private static string GetServiceArgs(bool podman, DenService service)
        {
            StringBuilder sb = new StringBuilder();
            string variablesPrefix = service.Name.Replace("-", "");

            string cmd;
            switch (service)
            {
                case DenLucidService denLucid:
                    cmd = podman ? "--healthcheck-command=\'curl -sS http://den-lucid:4242/health\'" : "--health-cmd=\'curl -sS http://den-lucid:4242/health\'";
                    sb.AppendLine($"${variablesPrefix}HealthCheck = \"{cmd}\"");
                    break;

                case DenServerService denServer1:
                    cmd = podman ? "--healthcheck-command=\'curl -sS http://den-server:10255/health\'" : "--health-cmd=\'curl -sS http://den-server:10255/health\'";
                    sb.AppendLine($"${variablesPrefix}HealthCheck = \"{cmd}\"");
                    break;
            }

            if (service.GetEnvironment(out List<string> envs))
            {
                sb.AppendLine($"${variablesPrefix}Envs = @(\"{string.Join("\",`\n   \"", envs)}\")");
            }

            if (service.GetVolumes(out List<string> volumes))
            {
                List<string> updatedVolumes = new List<string>();
                if (service is DenMongoService denMongo)
                {
                    foreach (string volume in volumes)
                    {
                        updatedVolumes.Add(volume);
                    }
                }
                else
                {
                    foreach (string volume in volumes)
                    {
                        updatedVolumes.Add($"$($currentPath){System.IO.Path.DirectorySeparatorChar}{volume}");
                    }
                }

                sb.AppendLine($"${variablesPrefix}Volumes = @(\"{string.Join("\",`\n    \"", updatedVolumes)}\")");
            }

            if (service.GetPorts(out List<string> ports))
            {
                sb.AppendLine($"${variablesPrefix}Ports = @(\'{string.Join("\',`\n  \'", ports)}\')");
            }

            if (service.GetCmd(out List<string> commands))
            {
                sb.AppendLine($"${variablesPrefix}Cmd = @(\'{string.Join("\',`\n    \'", commands)}\')");
            }

            return sb.ToString();
        }

        public static string script =
@"function WaykDen{{
param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('Start','Stop')]
    [string]$Action
)

    function PullImage{{
        foreach($image in $servicesImage){{
            {0} pull $image
        }}
    }}

    function CheckMongoVolume{{
        $exists = $({0} volume ls -qf ""name=den-mongodata"")

        if([string]::IsNullOrEmpty($exists)){{
            {0} volume create den-mongodata
        }}
    }}

    function IsContainerHealthy{{
        param(
            [string]$containerId
        )

        $s = 0
        $running = {0} inspect -f '{{{{.State.Running}}}}' $containerId
        $healthy = {0} inspect -f '{{{{.State.{4}.Status}}}}' $containerId
        while(($s -lt 30) -and !($healthy -like 'healthy') -and ($running -like 'true')){{
            Start-Sleep -Seconds 2
            $running = {0} inspect -f '{{{{.State.Running}}}}' $containerId
            $healthy = {0} inspect -f '{{{{.State.{4}.Status}}}}' $containerId
            $s = $s + 2
        }}

        return $healthy -like 'healthy'
    }}

    function StartContainer{{
        foreach($container in $servicesName){{
            $exists = $({0} ps -aqf ""name=$container"")

            if([string]::IsNullOrEmpty($exists)){{
                continue
            }}

            $running = {0} inspect -f '{{{{.State.Running}}}}' $container
            
            if ($running -and ($running -match 'false')){{
                $removed = {0} rm $container

                if($removed){{
                    Write-Host ""Removed $($container)""
                }} else {{
                    Write-Error -Message ""Error removing $($container)""
                }}
            }} elseif ($running -match 'true'){{
                Write-Warning -Message ""$container is running. Restart WaykDen""
                return
            }}
        }}

        CheckMongoVolume

        for($i = 0; $i -le $commands.Count - 1; $i++){{
            ${0}Run = ""$($baseRun)""
            {2}
            $var = Get-Variable ""$($commands[$i] + 'Envs')"" -ErrorAction SilentlyContinue | Select-Object *
            if($var){{
                $envs = $var.Value.split("" "")
                foreach($env in $envs){{
                    ${0}Run = ""$(${0}Run) -e $($env)""
                }}
            }}

            $var = Get-Variable ""$($commands[$i] + 'Volumes')"" -ErrorAction SilentlyContinue | Select-Object *
            if($var){{
                $volumes = $var.Value.split("" "")
                foreach($volume in $volumes){{
                    ${0}Run = ""$(${0}Run) -v $($volume)""
                }}
            }}

            $var = Get-Variable ""$($commands[$i] + 'Ports')"" -ErrorAction SilentlyContinue | Select-Object *
            if($var){{
                $ports = $var.Value.split("" "")
                foreach($port in $ports){{
                    ${0}Run = ""$(${0}Run) -p $($port)""
                }}
            }}

            $var = Get-Variable ""$($commands[$i] + 'HealthCheck')"" -ErrorAction SilentlyContinue | Select-Object *
            if($var){{
                $healthCmds = $var.Value.split("" "")
                foreach($healthCmd in $healthCmds){{
                    ${0}Run = ""$(${0}Run) $($healthCmd)""
                }}

                ${0}Run = ""${0}Run $($baseHealthCheck)""
            }}

            if($logDriver){{
                $driver = $logDriver.split("" "")
                if($syslogOptions -and $driver) {{
                    $options = $syslogOptions.split("" "")
                    $options += ""tag=$($servicesName[$i])""
                    ${0}Run = ""$(${0}Run) --log-driver $($driver)""
                    foreach($syslogOption in $options){{
                        ${0}Run = ""$(${0}Run) --log-opt $($syslogOption)""
                    }}
                }}
            }}

            ${0}Run = ""$(${0}Run) --name $($servicesName[$i]) $($servicesImage[$i])""           

            $var = Get-Variable ""$($commands[$i] + 'Cmd')"" -ErrorAction SilentlyContinue | Select-Object *
            if($var){{
                $cmds = $var.Value.split("" "")
                foreach($cmd in $cmds){{
                    ${0}Run = ""$(${0}Run) $($cmd)""
                }}
            }}

            Write-Host ""Starting $($servicesName[$i])""
            $id = Invoke-Expression ${0}Run
            $running = {0} inspect -f '{{{{.State.Running}}}}' $id
            if($running -like 'true'){{
                if(($servicesName[$i] -match 'den-lucid') -or ($servicesName[$i] -match 'den-server')){{
                    $healthy = IsContainerHealthy($id)

                    if($healthy -like 'true'){{
                        Write-Host ""$($servicesName[$i]) succesfully started""
                    }} else {{
                        Write-Error -Message ""Error starting $($servicesName[$i])""
                    }}
                }} else {{
                    Write-Host ""$($servicesName[$i]) succesfully started""
                }}
            }} else {{
                Write-Error -Message ""Error starting $($servicesName[$i])""
            }}
        }}
    }}
    {1}
    function StopContainer{{
        foreach($container in $servicesName){{
            $exists = $({0} ps -qf ""name=$container"")

            if([string]::IsNullOrEmpty($exists)){{
                continue
            }}

            $stopped = {0} stop $container

            if($stopped){{
                Write-Host ""$($stopped) stopped""
            }} else {{
                Write-Error -Message ""Error stopping $($container)""
            }}
        }}
    }}

    function StartWaykDen{{
        Write-Host 'Starting WaykDen'
        {3}
        PullImage
        StartContainer
        CheckOverrideDockerImages
    }}

    function StopWaykDen{{
        Write-Host 'Stopping WaykDen'
        StopContainer
    }}

    function CheckOverrideDockerImages{{
        if(!($DenImageDenLucidImage -eq $DenOriginalImageLucid)){{
            ShowDockerImageIsOverride $DenImageDenLucidImage $DenOriginalImageLucid
        }}
        if(!($DenImageDenMongoImage -eq $DenOriginalImageMongo)){{
            ShowDockerImageIsOverride $DenImageDenMongoImage $DenOriginalImageMongo
        }}
        if(!($DenImageDenPickyImage -eq $DenOriginalImagePicky)){{
            ShowDockerImageIsOverride $DenImageDenPickyImage $DenOriginalImagePicky
        }}
        if(!($DenImageDenServerImage -eq $DenOriginalImageServer)){{
            ShowDockerImageIsOverride $DenImageDenServerImage $DenOriginalImageServer
        }}
        if(!($DenImageDenTraefikImage -eq $DenOriginalImageTraefik)){{
            ShowDockerImageIsOverride $DenImageDenTraefikImage $DenOriginalImageTraefik
        }}
        if(!($DenImageDevolutionsJetImage -eq $DenOriginalImageJetImage)){{
            ShowDockerImageIsOverride $DenImageDevolutionsJetImage $DenOriginalImageJetImage
        }}
    }}

function ShowDockerImageIsOverride(
    [string] $stringOverride,
    [string] $original
){{
    Write-Warning ""The original docker image: `""$original`"" is overridden by `""$stringOverride`""""
}}

    if ($Action -match 'Start') {{
        StartWaykDen        
    }} else {{
        StopWaykDen
    }}
}}
";

        public static string CreateNetworkFunction = @"
    function CreateNetwork{
        $network = $(docker network ls -qf ""name=den-network"")
        if([string]::IsNullOrEmpty($network)){
            docker network create den-network
        }
    }
";

        public static string IPFunction = @"
            for($j = 0; $j -le $servicesIP.Count - 1; $j++){
                $podmanRun = ""$($podmanRun) --add-host=$($servicesName[$j]):$($servicesIP[$j])""
            }

            $podmanRun = ""$($podmanRun) --ip=$($servicesIP[$i])""
        ";
    }
}