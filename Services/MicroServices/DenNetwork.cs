using System.Threading.Tasks;
using Docker.DotNet.Models;
using WaykPS.Controllers;

namespace WaykPS.Services.MicroServices
{
    public class DenNetwork : DenService
    {
        private const string DENNETWORK_NAME = "den-network";
        public string NetworkId = string.Empty;
        public DenNetwork(DenServicesController controller)
        {
            this.DenServicesController = controller;
        }

        public void SetNetworkId(string id)
        {
            this.NetworkId = id;
        }

        public async Task CreateNetwork()
        {
            NetworksCreateParameters ncp = new NetworksCreateParameters
            (
                new NetworkCreate()
                {
                    Attachable = true,
                    CheckDuplicate = true,
                }
            ){ Name = DENNETWORK_NAME};

            NetworksCreateResponse response =  await this.DockerClient.Networks.CreateNetworkAsync(ncp);
            this.NetworkId = response.ID;
        }

        public async Task ConnectToNetwork(string container_id)
        {
            await this.DockerClient.Networks.ConnectNetworkAsync
            (
                this.NetworkId,
                new NetworkConnectParameters()
                {
                    Container = container_id
                }
            );
        }
    }
}