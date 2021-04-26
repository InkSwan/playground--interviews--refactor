using Landmark.FloodData.Processor.Model;

namespace Landmark.FloodData.Processor
{
    /// <summary>
    /// Represents a "Strategy" (see GoF) for determining what sort of action is required.
    /// The current implementation <see cref="HardCodedFloodActionStrategy"/> is likely be superseded by something
    /// configurable once more is known.
    /// </summary>
    public interface IFloodActionStrategy
    {
        FloodAction DetermineAction(Flood flood);
    }
}