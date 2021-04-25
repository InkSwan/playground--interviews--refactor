using System.Collections.Generic;
using Newtonsoft.Json;

namespace Landmark.FloodData.Processor.Model
{
	public class EnvironmentAgencyFloodAlertServicePayload
	{
		[JsonProperty(PropertyName = "items")]
		public IEnumerable<EnvironmentAgencyFloodAlert> Items { get; set; }
	}
}
