namespace WaykDen.Models
{
    public class DenConfig
    {
        public DenMongoConfigObject DenMongoConfigObject {get; set;}
        public DenPickyConfigObject DenPickyConfigObject {get; set;}
        public DenLucidConfigObject DenLucidConfigObject {get; set;}
        public DenRouterConfigObject DenRouterConfigObject {get; set;}
        public DenServerConfigObject DenServerConfigObject {get; set;}
        public DenTraefikConfigObject DenTraefikConfigObject {get; set;}
        public DenImageConfigObject DenImageConfigObject {get; set;}
        public DenDockerConfigObject DenDockerConfigObject {get; set;}
    }
}