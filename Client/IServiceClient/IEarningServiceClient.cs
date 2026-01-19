using CapManagement.Shared.DtoModels.EarningDtoModels;
using CapManagement.Shared.Models;
using CapManagement.Shared;

namespace CapManagement.Client.IServiceClient
{
    public interface IEarningServiceClient
    {

        Task<ApiResponse<EarningDto>> CreateEarningAsync(CreateEarningDto createEarningDto);


        Task<ApiResponse<PagedResponse<EarningDto>>> GetEarningAsync(Guid companyId, int pageNumber = 1, int pageSize = 10, string? orderBy = null, string? filter = null);




        Task<ApiResponse<bool>> ArchiveEarningAsync(Guid earningId, Guid companyId);

        Task<ApiResponse<bool>> RestoreEarningAsync(Guid earningId, Guid companyId);


        Task<ApiResponse<PagedResponse<EarningDto>>> GetArchivedEarningsAsync(Guid companyId, int pageNumber = 1, int pageSize = 10, string? orderBy = null, string? filter = null);

        Task<ApiResponse<EarningDto>> GetEarningByIdAsync(Guid earningId, Guid companyId);

        //Task<ApiResponse<bool>> UpdateEarningAsync(UpdateContractDto updateContractDto);

        Task<ApiResponse<List<EarningDto>>> CreateEarningsAsync(List<CreateEarningDto> earnings);

    }
}
