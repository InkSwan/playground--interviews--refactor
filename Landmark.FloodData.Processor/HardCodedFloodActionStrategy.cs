using Landmark.FloodData.Processor.Model;

namespace Landmark.FloodData.Processor
{
    /// <summary>
    /// An initial implementation of <see cref="IFloodActionStrategy"/>, it is likely be superseded by something
    /// configurable once more is known.
    /// </summary>
    public class HardCodedFloodActionStrategy : IFloodActionStrategy
    {
        public FloodAction DetermineAction(Flood flood)
        {
            switch (flood.EaAreaName.ToLower())
            {
                case "yorkshire":
                case "west midlands":
                {
                    return FloodAction.MonitorHourly;
                }
                case "east anglia":
                {
                    return FloodAction.MonitorDaily;
                }
                default:
                {
                    return FloodAction.Ignore;
                }
            }
        }
    }
}