using System;
using System.Collections.Generic;
using System.Linq;
using Landmark.FloodData.Processor.Model;

namespace Landmark.FloodData.Processor
{
    public class FloodDataProcessor
    {
        public IEnumerable<Flood> ProcessDataData(EnvironmentAgencyFloodAlertServicePayload environmentAgencyFloodAlerts)
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

        public IEnumerable<Flood> FilterData(IEnumerable<Flood> inputFloodData, string eaAreaName)
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