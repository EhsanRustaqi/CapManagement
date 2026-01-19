using CapManagement.Shared.DtoModels.SettlementDtoModels;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.EarningDtoModels;
using CapManagement.Shared.Models;

namespace CapManagement.Client.IServiceClient
{
    public interface ISettlmentClientService
    {


        Task<ApiResponse<SettlementDto>> CreateSettlementAsync(CreateSettlementDto dto);


        Task<ApiResponse<PagedResponse<SettlementDto>>> GetAllSettlementAsync(Guid companyId, int pageNumber = 1, int pageSize = 10, string? orderBy = null, string? filter = null);

        Task<ApiResponse<SettlementDto>> GetSettlementByIdAsync(Guid settlementId, Guid companyId);


        Task<byte[]> DownloadSettlementPdfAsync(Guid settlementId, Guid companyId);
    }
}
