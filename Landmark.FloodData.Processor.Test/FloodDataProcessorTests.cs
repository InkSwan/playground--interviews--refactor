using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Landmark.FloodData.Processor.Model;

namespace Landmark.FloodData.Processor.Test
{
    public class FloodDataProcessorTests
    {
        [Test]
        public void ProcessData_WithNullPayload_ReturnsEmptyList()
        {
            var target = new FloodDataProcessor();

            var result =target.ProcessDataData(null);

            Assert.IsNotNull(result);
            Assert.AreEqual(new List<Flood>(), result);
        }

        [Test]
        public void ProcessData_WithEmptyPayload_ReturnsEmptyList()
        {
            var target = new FloodDataProcessor();
            var emptyPayload = new EnvironmentAgencyFloodAlertServicePayload();

            var result = target.ProcessDataData(emptyPayload);

            Assert.IsNotNull(result);
            Assert.AreEqual(new List<Flood>(), result);
        }

        [Test]
        public void ProcessData_WithEmptyPayloadItems_ReturnsEmptyList()
        {
            var target = new FloodDataProcessor();
            var emptyPayload = new EnvironmentAgencyFloodAlertServicePayload
            {
                Items = new List<EnvironmentAgencyFloodAlert>()
            };

            var result = target.ProcessDataData(emptyPayload);

            Assert.IsNotNull(result);
            Assert.AreEqual(new List<Flood>(), result);
        }

        [Test]
        public void ProcessData_WithPayloadWithSingleItem_ReturnsSingleFlood()
        {
            var target = new FloodDataProcessor();
            var payload = new EnvironmentAgencyFloodAlertServicePayload
            {
                Items = new List<EnvironmentAgencyFloodAlert>
                {
                    new EnvironmentAgencyFloodAlert
                    {
                        Id = "http://environment.data.gov.uk/flood-monitoring/id/floods/104684",
                        EaAreaName = "Cornwall",
                        FloodAreaId = "114WAFT1W10A00",
                        TimeRaised = new DateTime(2021,04,26,17,52,0),
                        SeverityLevel = 4
                    }
                }
            };

            var result = target.ProcessDataData(payload);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            var first = result.First();
            Assert.AreEqual("104684", first.Id);
            Assert.AreEqual("Cornwall", first.EaAreaName);
            Assert.AreEqual("114WAFT1W10A00", first.FloodAreaId);
            Assert.AreEqual(new DateTime(2021, 04, 26, 17, 52, 0), first.TimeRaised);
            Assert.AreEqual(SeverityLevel.Yellow, first.Severity);
            Assert.AreEqual(FloodAction.Ignore, first.Action);
        }

        [Test]
        public void ProcessData_WithSeveralItems_ReturnsList()
        {
            var target = new FloodDataProcessor();
            var payload = new EnvironmentAgencyFloodAlertServicePayload
            {
                Items = new List<EnvironmentAgencyFloodAlert>
                {
                    new EnvironmentAgencyFloodAlert
                    {
                        Id = "http://environment.data.gov.uk/flood-monitoring/id/floods/1",
                        EaAreaName = "Cornwall",
                    },
                    new EnvironmentAgencyFloodAlert
                    {
                        Id = "http://environment.data.gov.uk/flood-monitoring/id/floods/2",
                        EaAreaName = "West",
                    },
                    new EnvironmentAgencyFloodAlert
                    {
                        Id = "http://environment.data.gov.uk/flood-monitoring/id/floods/3",
                        EaAreaName = "Eastern",
                    }
                }
            };

            var result = target.ProcessDataData(payload);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
            CollectionAssert.AreEqual(new[] {"1", "2", "3"}, result.Select(flood => flood.Id));
        }

        [TestCase("Cornwall", FloodAction.Ignore)]
        [TestCase("Yorkshire", FloodAction.MonitorHourly)]
        [TestCase("East Anglia", FloodAction.MonitorDaily)]
        [TestCase("Cornwall", FloodAction.Ignore)]
        public void ProcessData_WithCertainRegions_SetsAction(string region, FloodAction expectedAction)
        {
            var target = new FloodDataProcessor();
            var payload = new EnvironmentAgencyFloodAlertServicePayload
            {
                Items = new List<EnvironmentAgencyFloodAlert>
                {
                    new EnvironmentAgencyFloodAlert
                    {
                        Id = "http://environment.data.gov.uk/flood-monitoring/id/floods/104684",
                        EaAreaName = region,
                    }
                }
            };

            var result = target.ProcessDataData(payload);

            Assert.AreEqual(expectedAction, result.First().Action);
        }

        [Test]
        public void Filter_WithRegionInList_ReturnsFloodForRegion()
        {
            var target = new FloodDataProcessor();
            var floods = new List<Flood>
            {
                new Flood {EaAreaName = "Cornwall"},
                new Flood {EaAreaName = "West"},
                new Flood {EaAreaName = "East"}
            };

            var result = target.FilterData(floods, "West");

            CollectionAssert.AreEqual(new []{"West"}, result.Select(flood => flood.EaAreaName));
        }

        [Test]
        public void Filter_WithRegionNotInList_ReturnsNoFloods()
        {
            var target = new FloodDataProcessor();
            var floods = new List<Flood>
            {
                new Flood {EaAreaName = "Cornwall"},
                new Flood {EaAreaName = "West"},
                new Flood {EaAreaName = "East"}
            };

            var result = target.FilterData(floods, "NotThere");

            Assert.AreEqual(0, result.Count());
        }
    }
}