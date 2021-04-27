using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Landmark.FloodData.Processor;
using Landmark.FloodData.Processor.Model;
using Newtonsoft.Json;

namespace Landmark.FloodData.Gateway
{
    public class EnvironmentAgencyGateway : IEnvironmentAgencyGateway
    {
        private readonly IHttpClientFactory _clientFactory;

        public EnvironmentAgencyGateway(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<EnvironmentAgencyFloodAlertServicePayload> GetEnvironmentAgencyData()
        {
            using var client = _clientFactory.CreateClient("EnvironmentAgency");
            var response = await client.GetAsync("flood-monitoring/id/floods");
            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            var environmentAgencyApiResponseContent =
                await response.Content.ReadAsStringAsync();

            var environmentAgencyFloodAlerts =
                JsonConvert.DeserializeObject<EnvironmentAgencyFloodAlertServicePayload>(
                    environmentAgencyApiResponseContent);

            return environmentAgencyFloodAlerts;
        }
    }
}