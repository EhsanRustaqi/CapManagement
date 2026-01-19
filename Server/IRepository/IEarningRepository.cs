using CapManagement.Shared.Models;

namespace CapManagement.Server.IRepository
{
    public interface IEarningRepository
    {
        Task <PageResult<Earning>> GetAllEarningsAsync(int pageNumber, int pageSize,
            Guid companyId, string? orderBy, string? filter);

        Task<PageResult<Earning>> GetArchivedEarningsAsync(int pageNumber, int pageSize,
            Guid companyId, string? orderBy, string? filter);

       Task<Earning> CreateEarningAysnc(Earning earning);

        Task<bool> UpdateEarningAsync(Earning earning);
        Task<bool> ArchiveEarningAsync(Guid earningId, Guid companyId);

        Task<bool> RestoreEarningAsync(Guid earningId, Guid companyId);

        Task<Earning?> GetEarningByIdAsync(Guid earningId, Guid companyId);



        Task<List<Earning>> CreateEarningsAsync(List<Earning> earnings);


    }
}
