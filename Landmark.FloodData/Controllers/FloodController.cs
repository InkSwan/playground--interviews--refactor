using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Landmark.FloodData.Processor;

namespace Landmark.FloodData.Controllers
{
    [ApiController]
	public class FloodController : ControllerBase
	{
        private readonly IEnvironmentAgencyGateway _environmentAgencyGateway;
        private readonly FloodDataProcessor _floodDataProcessor;

        public FloodController(IEnvironmentAgencyGateway environmentAgencyGateway, FloodDataProcessor floodDataProcessor)
        {
            _environmentAgencyGateway = environmentAgencyGateway;
            _floodDataProcessor = floodDataProcessor;
        }

        [HttpGet]
        [Route("Flood/{region?}")]
        public async Task<ActionResult> Get(string region)
        {
            var environmentAgencyFloodAlerts = await _environmentAgencyGateway.GetEnvironmentAgencyData();
            if (environmentAgencyFloodAlerts == null)
                return StatusCode((int)HttpStatusCode.ServiceUnavailable);
            
            var processedData = _floodDataProcessor.ProcessDataData(environmentAgencyFloodAlerts);

            if (region == null)
                return Ok(processedData);

            var filteredData = _floodDataProcessor.FilterData(processedData, region);
            return Ok(filteredData);
        }
    }
}
