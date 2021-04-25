using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Landmark.FloodData.Models
{
    public class EnvironmentAgencyGateway : IEnvironmentAgencyGateway
    {
        private readonly HttpMessageHandler _httpMessageHandler;

        public EnvironmentAgencyGateway(HttpMessageHandler httpMessageHandler)
        {
            _httpMessageHandler = httpMessageHandler;
        }

        public async Task<EnvironmentAgencyFloodAlertServicePayload> GetEnvironmentAgencyData()
        {
            using var client = new HttpClient(_httpMessageHandler) {BaseAddress = new Uri("http://environment.data.gov.uk")};
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