using CapManagement.Shared.Models;

namespace CapManagement.Server.IRepository
{
    public interface ISettlementRepository
    {
        Task<Settlement> CreateSettlementWithEarningsAsync(Settlement settlement, List<Earning> earnings);
        Task<PageResult<Settlement>> GetAllSettlmentsAsync(int pageNumber, int pageSize,
            Guid companyId, string? orderBy, string? filter);

        Task<PageResult<Settlement>> GetArchivedSettlmentsAsync(int pageNumber, int pageSize,
            Guid companyId, string? orderBy, string? filter);

        Task<Settlement> CreateSettlementAysnc(Settlement settlment);

        Task<bool> UpdateSettlementAsync(Settlement settlment);
        Task<bool> ArchiveSettlmentAsync(Guid settlmentId, Guid companyId);

        Task<bool> RestoreSettlementAsync(Guid settlmentId, Guid companyId);

        Task<Settlement?> GetSettlementByIdAsync(Guid settlmentId, Guid companyId);

        Task LoadNavigationForRecalcAsync(Settlement settlement);

    }
}
