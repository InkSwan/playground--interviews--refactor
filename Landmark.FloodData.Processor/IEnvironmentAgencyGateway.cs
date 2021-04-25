using System.Threading.Tasks;
using Landmark.FloodData.Processor.Model;

namespace Landmark.FloodData.Processor
{
    public interface IEnvironmentAgencyGateway
    {
        Task<EnvironmentAgencyFloodAlertServicePayload> GetEnvironmentAgencyData();
    }
}