using CapManagement.Shared.DtoModels.ContractDto;
using CapManagement.Shared.DtoModels.EarningDtoModels;
using CapManagement.Shared.Models;
using CapManagement.Shared;

namespace CapManagement.Server.IService
{
    public interface IEarningService
    {

        Task<ApiResponse<PagedResponse<EarningDto>>> GetAllEarningAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter);

        Task<ApiResponse<EarningDto>> CreateEarningAsync(CreateEarningDto earningDto, string userId);



        Task<EarningDto> GetEarningByIdAsync(Guid earningId, Guid companyId);

        Task<ApiResponse<bool>> ArchiveEarningtAsync(Guid earningId, Guid companyId);

        Task<ApiResponse<bool>> RestoreEarningAsync(Guid earningId, Guid companyId);

        //Task<ApiResponse<bool>> UpdateEarningAsync(UpdateContractDto updateContractDto);

        Task<ApiResponse<PagedResponse<EarningDto>>> GetArchivedEarningsAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter);

        Task<ApiResponse<List<EarningDto>>> CreateEarningsAsync(List<CreateEarningDto> dtos);
    }   
}
