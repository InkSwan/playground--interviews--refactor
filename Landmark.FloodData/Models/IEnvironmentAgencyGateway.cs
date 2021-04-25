using System.Threading.Tasks;

namespace Landmark.FloodData.Models
{
    public interface IEnvironmentAgencyGateway
    {
        Task<EnvironmentAgencyFloodAlertServicePayload> GetEnvironmentAgencyData();
    }
}