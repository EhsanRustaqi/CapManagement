using CapManagement.Shared.Models;

namespace CapManagement.Server.IRepository
{
    public interface IContractRepository
    {

        Task<PageResult<Contract>> GetAllContractAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter);


        Task<PageResult<Contract>> GetAllInActiveContractAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter);

        Task<PageResult<Contract>> GetArchivedContractAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter);


        Task<Contract> CreateContractAsync(Contract contract);

        Task<bool> UpdateContractAsync(Contract contract);

        Task<bool> ArchiveContractAsync(Guid contractId, Guid companyId);

        Task<bool> RestoreContractAsync(Guid contractId, Guid companyId);


        Task<Contract?> GetContractByIdAsync(Guid contractId, Guid companyId);

        Task<Contract?> GetContractByCarAsync(string numberPlate, Guid companyId
            );


        Task<bool> UpdateContractStatusAsync(Guid contractId, Guid companyId, ContractStatus newStatus);


        Task<byte[]?> GetContractPdfAsync(Guid contractId, Guid companyId);
        Task<Contract?> GetActiveContractByCarAsync(Guid carId, Guid companyId);


        Task<Contract?> GetActiveContractAsync(Guid contractId);
    }
}
