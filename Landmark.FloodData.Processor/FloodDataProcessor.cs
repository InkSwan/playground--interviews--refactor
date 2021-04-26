using System;
using System.Collections.Generic;
using System.Linq;
using Landmark.FloodData.Processor.Model;

namespace Landmark.FloodData.Processor
{
    public class FloodDataProcessor
    {
        private readonly IFloodActionStrategy _floodActionStrategy;

        public FloodDataProcessor(IFloodActionStrategy floodActionStrategy)
        {
            _floodActionStrategy = floodActionStrategy;
        }

        public List<Flood> ProcessDataData(EnvironmentAgencyFloodAlertServicePayload environmentAgencyFloodAlerts)
        {
            if (environmentAgencyFloodAlerts?.Items == null)
                return new List<Flood>(); 

            return environmentAgencyFloodAlerts.Items
                .Select(ProcessFloodItem)
                .ToList();
        }

        private Flood ProcessFloodItem(EnvironmentAgencyFloodAlert item)
        {
            var flood = new Flood
            {
                Id = LastSegmentOfUri(item.Id),
                Region = item.EaRegionName,
                FloodAreaId = item.FloodAreaId,
                EaAreaName = item.EaAreaName,
                TimeRaised = item.TimeRaised,
                Severity = (SeverityLevel) item.SeverityLevel
            };

            flood.Action = _floodActionStrategy.DetermineAction(flood);

            return flood;
        }

        public List<Flood> FilterData(IEnumerable<Flood> inputFloodData, string eaAreaName)
        {
            return inputFloodData
                .Where(item => string.Equals(item.EaAreaName, eaAreaName, StringComparison.CurrentCultureIgnoreCase))
                .ToList();
        }

        private static string LastSegmentOfUri(string id)
        {
            return id.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last();
        }
    }
}