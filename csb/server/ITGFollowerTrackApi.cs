using csb.server.track_dtos;
using System.Threading.Tasks;

namespace csb.server
{
    public interface ITGFollowerTrackApi
    {
        Task EnqueueInviteLink(string geotag, string link);
        Task<leadDataDto> GetLeadData(string link);
        Task<int> GetInviteLinksAvailable(string geotag);
    }
}
