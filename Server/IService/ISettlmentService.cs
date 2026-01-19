using CapManagement.Shared;
using CapManagement.Shared.DtoModels.EarningDtoModels;
using CapManagement.Shared.DtoModels.SettlementDtoModels;
using CapManagement.Shared.Models;

namespace CapManagement.Server.IService
{
    public interface ISettlmentService
    {

        Task<ApiResponse<SettlementDto>> CreateSettlementAsync(CreateSettlementDto dto);


        Task<ApiResponse<PagedResponse<SettlementDto>>> GetAllSettlementAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter);
             
        Task<SettlementDto> GetSettlementByIdAsync(Guid settlmentId, Guid companyId);

      
    }
}
