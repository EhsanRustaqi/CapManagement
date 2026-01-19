using CapManagement.Shared;
using CapManagement.Shared.DtoModels.CarDtoModels;
using CapManagement.Shared.DtoModels.ContractDto;
using CapManagement.Shared.Models;

namespace CapManagement.Client.IServiceClient
{
    public interface IContractServiceClient
    {


        Task<ApiResponse<ContractDto>> CreateContractAsync(CreateContractDto createContractDto, Stream? pdfStream = null);


        Task<ApiResponse<PagedResponse<ContractDto>>> GetContractAsync(Guid companyId, int pageNumber = 1, int pageSize = 10, string? orderBy = null, string? filter = null);

        Task<ApiResponse<byte[]>> GetContractPdfAsync(Guid contractId, Guid companyId);


        Task<ApiResponse<bool>> ArchiveContractAsync(Guid contractId, Guid companyId);

        Task<ApiResponse<bool>> RestoreContractAsync(Guid contractId, Guid companyId);


        Task<ApiResponse<PagedResponse<ContractDto>>> GetArchivedContractsAsync(Guid companyId, int pageNumber = 1, int pageSize = 10, string? orderBy = null, string? filter = null);

        Task<ApiResponse<ContractDto>> GetContractByIdAsync(Guid contractId, Guid companyId);

        Task<ApiResponse<bool>> UpdateContractAsync(UpdateContractDto updateContractDto);



    }
}
