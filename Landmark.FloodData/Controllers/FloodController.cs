using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Landmark.FloodData.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Landmark.FloodData.Controllers
{
    [ApiController]
	public class FloodController : ControllerBase
	{
        private readonly HttpMessageHandler _httpMessageHandler;

        public FloodController(HttpMessageHandler httpMessageHandler)
        {
            _httpMessageHandler = httpMessageHandler;
        }

        [Route("Flood/{region?}")]
        [Produces("application/xml")]
        public async Task<ActionResult> Get(string region)
        {
            var environmentAgencyFloodAlerts = await GetEnvironmentAgencyData();
            if (environmentAgencyFloodAlerts == null)
                return NotFound();
            
            var processedData = ProcessDataData(environmentAgencyFloodAlerts);

            if (region == null)
                return Ok(processedData);

            var filteredData = FilterData(processedData, region);
            return Ok(filteredData);
        }

		private async Task<EnvironmentAgencyFloodAlertServicePayload> GetEnvironmentAgencyData()
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

		private IEnumerable<Flood> ProcessDataData(EnvironmentAgencyFloodAlertServicePayload environmentAgencyFloodAlerts)
		{
			var floodData = new List<Flood>();

			if (environmentAgencyFloodAlerts == null || !environmentAgencyFloodAlerts.Items.Any())
			{
				return floodData;
			}

			foreach (var item in environmentAgencyFloodAlerts.Items)
			{
				var itemId = item.Id.Replace("http://environment.data.gov.uk/flood-monitoring/id/floods/", "");

				var flood = new Flood
				{
					Id = itemId,
					Region = item.EaRegionName,
					FloodAreaId = item.FloodAreaId,
					EaAreaName = item.EaAreaName,
					TimeRaised = item.TimeRaised,
					Severity = (SeverityLevel) item.SeverityLevel
				};

				switch (item.EaAreaName.ToLower())
				{
					case "yorkshire":
					case "west midlands":
					{
						flood.Action = FloodAction.MonitorHourly;
						break;
					}
					case "east anglia":
					{
						flood.Action = FloodAction.MonitorDaily;
						break;
					}
					default:
					{
						flood.Action = FloodAction.Ignore;
						break;
					}
				}

				floodData.Add(flood);
			}

			return floodData;
		}

		private IEnumerable<Flood> FilterData(IEnumerable<Flood> inputFloodData, string eaAreaName)
		{
			var floodData = new List<Flood>();

			foreach (var item in inputFloodData)
			{
				if (!string.Equals(item.EaAreaName, eaAreaName, StringComparison.CurrentCultureIgnoreCase))
				{
					continue;
				}

				floodData.Add(item);
			}

			return floodData;
		}
	}
}
