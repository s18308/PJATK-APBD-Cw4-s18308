using PJATK_APBD_Cw4_s18308.DTOs;

namespace PJATK_APBD_Cw4_s18308.Services;

public interface IDbService
{
    Task<ICollection<PcResponse>> GetAllPcsAsync(CancellationToken cancellationToken);
    Task<PcWithComponentsResponse> GetPcWithComponentsAsync(int id, CancellationToken cancellationToken);
    Task<PcResponse> AddPcAsync(PcRequest request, CancellationToken cancellationToken);
    Task UpdatePcAsync(int id, PcRequest request, CancellationToken cancellationToken);
    Task DeletePcAsync(int id, CancellationToken cancellationToken);
}