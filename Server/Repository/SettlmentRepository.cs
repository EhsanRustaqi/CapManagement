using CapManagement.Client.Pages.Driver.Driver;
using CapManagement.Server.DbContexts;
using CapManagement.Server.IRepository;
using CapManagement.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
namespace CapManagement.Server.Repository
{
    public class SettlmentRepository : ISettlementRepository
    {

        private readonly FleetDbContext _context;
        public SettlmentRepository(FleetDbContext context)
        {
            _context = context;
        }
        public Task<bool> ArchiveSettlmentAsync(Guid settlmentId, Guid companyId)
        {
            throw new NotImplementedException();
        }

        public async Task<Settlement> CreateSettlementAysnc(Settlement settlment)
        {
            _context.Settlements.Add(settlment);
            await _context.SaveChangesAsync();
            return settlment;
        }

        //public async Task<Settlement> CreateSettlementWithEarningsAsync(Settlement settlement, List<Earning> earnings)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();  // Atomic

        //    try
        //    {
        //        // 1. Insert settlement FIRST (commit ID)
        //        _context.Settlements.Add(settlement);
        //        await _context.SaveChangesAsync();  // ID now in DB

        //        // 2. Link earnings (FK valid)
        //        if (earnings?.Any() == true)
        //        {
        //            // For new (Empty ID): AddRange
        //            var newEarnings = earnings.Where(e => e.EarningId == Guid.Empty).ToList();
        //            if (newEarnings.Any())
        //            {
        //                _context.Earnings.AddRange(newEarnings);
        //            }

        //            // For existing: UpdateRange
        //            var existingEarnings = earnings.Where(e => e.EarningId != Guid.Empty).ToList();
        //            if (existingEarnings.Any())
        //            {
        //                foreach (var e in existingEarnings)
        //                {
        //                    e.SettlementId = settlement.SettlementId;
        //                }
        //                _context.Earnings.UpdateRange(existingEarnings);
        //            }

        //            await _context.SaveChangesAsync();  // Commit links
        //        }

        //        await transaction.CommitAsync();

        //        return settlement;
        //    }
        //    catch
        //    {
        //        await transaction.RollbackAsync();
        //        throw;  // For service catch/log
        //    }
        //}
        public async Task<Settlement> CreateSettlementWithEarningsAsync(Settlement settlement, List<Earning> earnings)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1) Insert settlement FIRST
                _context.Settlements.Add(settlement);
                await _context.SaveChangesAsync();
                // SettlementId is real in DB now

                // 2) Insert all earnings AFTER settlement exists
                foreach (var e in earnings)
                {
                    e.SettlementId = settlement.SettlementId; // ensure FK is correct
                }

                _context.Earnings.AddRange(earnings);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return settlement;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PageResult<Settlement>> GetAllSettlmentsAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));
            }

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, Math.Min(pageSize, 100));

            var query = _context.Settlements
            .Include(s => s.Contract)
            .ThenInclude(c => c.Driver)
           .Where(d => d.CompanyId == companyId && d.IsActive)
            .AsNoTracking();

            // Apply filtering
            if (!string.IsNullOrEmpty(filter))
            {
                try
                {
                    query = query.Where(filter);
                }
                catch (Exception ex)
                {
                    // Log the error and return empty result to prevent crashing
                    Console.WriteLine($"Filter error: {ex.Message}");
                    return new PageResult<Settlement>
                    {
                        Items = new List<Settlement>(),
                        TotalCount = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(orderBy))
            {
                try
                {
                    query = query.OrderBy(orderBy);
                }
                catch (Exception ex)
                {
                    // Log the error and fall back to default sort
                    Console.WriteLine($"Sort error: {ex.Message}");
                    query = query.OrderBy(c => c.SettlementId); // Default sort
                }
            }
            else
            {
                query = query.OrderBy(c => c.SettlementId); // Default sort by CarId
            }

            // Remove redundant OrderBy(d => d.Brand)
            var totalCount = await query.CountAsync();
            var settlments = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageResult<Settlement>
            {
                Items = settlments,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public Task<PageResult<Settlement>> GetArchivedSettlmentsAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
        {
            throw new NotImplementedException();
        }

        public async Task<Settlement?> GetSettlementByIdAsync(Guid settlmentId, Guid companyId)
        {
            if (settlmentId == Guid.Empty)
                throw new ArgumentException("Settlement ID cannot be empty.", nameof(settlmentId));

            if (companyId == Guid.Empty)
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

            //return await _context.Settlements
            //    .Include(s => s.Earnings)
            //    .FirstOrDefaultAsync(s =>
            //        s.SettlementId == settlmentId &&
            //        s.CompanyId == companyId &&
            //        s.IsActive);
            return await _context.Settlements
    .Include(s => s.Earnings)
    .Include(s => s.Contract)
        .ThenInclude(c => c.Driver)
    .Include(s => s.Contract)
        .ThenInclude(c => c.Car)
    .FirstOrDefaultAsync(s =>
        s.SettlementId == settlmentId &&
        s.CompanyId == companyId &&
        s.IsActive);


        }

        public async Task LoadNavigationForRecalcAsync(Settlement settlement)
        {
            if (settlement == null)
            {
                throw new ArgumentNullException(nameof(settlement));
            }

            // Load Contract (for PaymentAmount in RecalculateTotals)
            await _context.Entry(settlement).Reference(s => s.Contract).LoadAsync();

            // Load Earnings (for sums in RecalculateTotals)
            await _context.Entry(settlement).Collection(s => s.Earnings).LoadAsync();
        }

        public Task<bool> RestoreSettlementAsync(Guid settlmentId, Guid companyId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateSettlementAsync(Settlement settlment)
        {
            _context.Settlements.Update(settlment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
