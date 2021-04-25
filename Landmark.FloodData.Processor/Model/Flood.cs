using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Landmark.FloodData.Processor.Model
{
	public class Flood
	{
		public string Id { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public SeverityLevel Severity { get; set; }

		public string Region { get; set; }

		public string FloodAreaId { get; set; }
		
		public string EaAreaName { get; set; }

		public DateTime TimeRaised { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public FloodAction Action { get; set; }
	}
}