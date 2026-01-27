using CapManagement.Shared.DtoModels.CarDtoModels;
using CapManagement.Shared.Models;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.ContractDto;

namespace CapManagement.Server.IService
{
    public interface IContractService
    {
        Task<ApiResponse<PagedResponse<ContractDto>>> GetAllContractAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter);


        Task<ApiResponse<PagedResponse<ContractDto>>> GetAllInActiveContractAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter);


        Task<ApiResponse<ContractDto>> CreateContractAsync(CreateContractDto contractDto, string userId);

        Task<ApiResponse<byte[]>> GetContractPdfAsync(Guid contractId, Guid companyId);

        Task<ContractDto> GetContractByIdAsync(Guid contractId, Guid companyId);

        Task<ApiResponse<bool>> ArchiveContractAsync(Guid contractId, Guid companyId);

        Task<ApiResponse<bool>> RestoreContractAsync(Guid contractId, Guid companyId);

        Task<ApiResponse<bool>> UpdateContractAsync(UpdateContractDto updateContractDto);
        
        Task<ApiResponse<PagedResponse<ContractDto>>> GetArchivedContractsAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter);

        Task<ApiResponse<bool>> ExpireContractAsync(Guid contractId, Guid companyId);


    }
}
