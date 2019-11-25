using System;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace WaykDen.Models
{
    public class DenConfig
    {
        public DenMongoConfigObject DenMongoConfigObject {get; set;}
        public DenPickyConfigObject DenPickyConfigObject {get; set;}
        public DenLucidConfigObject DenLucidConfigObject {get; set;}
        public DenServerConfigObject DenServerConfigObject {get; set;}
        public DenTraefikConfigObject DenTraefikConfigObject {get; set;}
        public DenImageConfigObject DenImageConfigObject {get; set;}
        public DenDockerConfigObject DenDockerConfigObject {get; set;}

        public string ConvertToPwshParameters(Platforms platform)
        {
            StringBuilder sb = new StringBuilder();
            bool syslog = false;
            foreach(PropertyInfo denConfigProperty in this.GetType().GetProperties())
            {
                var denConfigPropertyValue = this.GetType().GetProperty(denConfigProperty.Name).GetValue(this);
                foreach(PropertyInfo childProperty in denConfigPropertyValue.GetType().GetProperties())
                {
                    if(!Attribute.IsDefined(childProperty, typeof(JsonIgnoreAttribute)))
                    {
                        var value = childProperty.GetValue(denConfigPropertyValue);
                        if(string.IsNullOrEmpty(value?.ToString()))
                        {
                            continue;
                        }
                        string parameterName = $"${denConfigProperty.Name.Replace("ConfigObject", string.Empty)}{childProperty.Name}";
                        sb.AppendLine($"{parameterName} = \'{value}\'");
                        childProperty.SetValue(denConfigPropertyValue, $"$({parameterName})");
                    }
                }

                if (!string.IsNullOrEmpty(this.DenDockerConfigObject.SyslogServer) && !syslog)
                {
                    syslog = true;    
                }
            }

            string DenOriginalImageLucid = platform == Platforms.Linux ? DenImageConfigObject.LinuxDenLucidImage : DenImageConfigObject.WindowsDenLucidImage;
            string DenOriginalImageMongo = platform == Platforms.Linux ? DenImageConfigObject.LinuxDenMongoImage : DenImageConfigObject.WindowsDenMongoImage;
            string DenOriginalImagePicky = platform == Platforms.Linux ? DenImageConfigObject.LinuxDenPickyImage : DenImageConfigObject.WindowsDenPickyImage;
            string DenOriginalImageServer = platform == Platforms.Linux ? DenImageConfigObject.LinuxDenServerImage : DenImageConfigObject.WindowsDenServerImage;
            string DenOriginalImageTraefik = platform == Platforms.Linux ? DenImageConfigObject.LinuxDenTraefikImage : DenImageConfigObject.WindowsDenTraefikImage;
            string DenOriginalImageJetImage = platform == Platforms.Linux ? DenImageConfigObject.LinuxDevolutionsJetImage : DenImageConfigObject.WindowsDevolutionsJetImage;

            sb.AppendLine($"$DenOriginalImageLucid = \'{DenOriginalImageLucid}\'");
            sb.AppendLine($"$DenOriginalImageMongo = \'{DenOriginalImageMongo}\'");
            sb.AppendLine($"$DenOriginalImagePicky = \'{DenOriginalImagePicky}\'");
            sb.AppendLine($"$DenOriginalImageServer = \'{DenOriginalImageServer}\'");
            sb.AppendLine($"$DenOriginalImageTraefik = \'{DenOriginalImageTraefik}\'");
            sb.AppendLine($"$DenOriginalImageJetImage = \'{DenOriginalImageJetImage}\'");

            if (syslog)
            {
                sb.AppendLine("$logDriver = \'syslog\'");
                sb.AppendLine($"$syslogOptions = @(\'syslog-address={this.DenDockerConfigObject.SyslogServer}\',\'syslog-format=rfc5424\',\'syslog-facility=daemon\')");
            }

            return sb.ToString();
        }
    }
}