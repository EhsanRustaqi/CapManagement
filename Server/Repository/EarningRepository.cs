using CapManagement.Server.DbContexts;
using CapManagement.Server.IRepository;
using CapManagement.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
namespace CapManagement.Server.Repository
{
    public class EarningRepository : IEarningRepository
    {

        private readonly FleetDbContext _context;
        public EarningRepository(FleetDbContext context)
        {
            _context = context;
        }



        public async Task<bool> ArchiveEarningAsync(Guid earningId, Guid companyId)
        {
            if (earningId == Guid.Empty)
            {
                throw new ArgumentException("Contract ID cannot be empty.", nameof(earningId));
            }

            if (companyId == Guid.Empty)
            {
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));
            }

            var contract = await _context.Earnings
                .Where(c => c.EarningId == earningId && c.CompanyId == companyId && c.IsActive)
                .FirstOrDefaultAsync();

            if (contract == null)
            {
                return false;
            }

            contract.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }




        public async Task<Earning> CreateEarningAysnc(Earning earning)
        {
          _context.Earnings.Add(earning);
            await _context.SaveChangesAsync();
            return earning;
        }

        public async Task<List<Earning>> CreateEarningsAsync(List<Earning> earnings)
        {
            if (earnings == null || !earnings.Any())
            {
                return new List<Earning>();  // Or throw ArgumentException
            }

            // Optional: Business validation (e.g., no duplicates)
            var duplicateIds = earnings.GroupBy(e => e.EarningId).Where(g => g.Count() > 1).ToList();
            if (duplicateIds.Any())
            {
                throw new InvalidOperationException("Duplicate earning IDs detected.");
            }

            _context.Earnings.AddRange(earnings);  // Batch add
            await _context.SaveChangesAsync();     // Single save—generates IDs atomically

            return earnings;  // Return updated list (IDs now set)
        }

        public async Task<PageResult<Earning>> GetAllEarningsAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));
            }
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, Math.Min(pageSize, 100));

            var query = _context.Earnings
                .Where(e => e.CompanyId == companyId && e.IsActive)
                 .Include(e => e.Contract)
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
                    return new PageResult<Earning>
                    {
                        Items = new List<Earning>(),
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
                    query = query.OrderBy(c => c.EarningId); // Default sort
                }
            }
            else
            {
                query = query.OrderBy(c => c.EarningId); // Default sort by CarId
            }

            // Remove redundant OrderBy(d => d.Brand)
            var totalCount = await query.CountAsync();
            var earnings = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageResult<Earning>
            {
                Items = earnings,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

        }



        public async Task<PageResult<Earning>> GetArchivedEarningsAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
        {
            if (companyId == Guid.Empty)
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, Math.Min(pageSize, 100));

            // 🔹 Base query — archived (inactive or deleted) earnings for a company
            var query = _context.Earnings
                .IgnoreQueryFilters()
                .Include(c => c.Contract)
                .Where(c => (c.IsDeleted || !c.IsActive) && c.CompanyId == companyId)
                .AsNoTracking();

            // 🔹 Apply filtering (dynamic LINQ)
            if (!string.IsNullOrEmpty(filter))
            {
                try
                {
                    query = query.Where(filter);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Filter error: {ex.Message}");
                    return new PageResult<Earning>
                    {
                        Items = new List<Earning>(),
                        TotalCount = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }

            // 🔹 Apply sorting (dynamic LINQ)
            if (!string.IsNullOrEmpty(orderBy))
            {
                try
                {
                    query = query.OrderBy(orderBy);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Sort error: {ex.Message}");
                    query = query.OrderBy(c => c.EarningId);
                }
            }
            else
            {
                query = query.OrderBy(c => c.EarningId);
            }

            // 🔹 Pagination
            var totalCount = await query.CountAsync();
            var earnings = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 🔹 Return paged result
            return new PageResult<Earning>
            {
                Items = earnings,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

        }

        public async Task<Earning?> GetEarningByIdAsync(Guid earningId, Guid companyId)
        {
            if (earningId == Guid.Empty)
            {
                throw new ArgumentException("Earning ID cannot be empty.", nameof(earningId));  // FIXED: Correct message
            }
            if (companyId == Guid.Empty)
            {
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));  // Already correct
            }
            return await _context.Earnings
                .Include(c => c.Contract)
                .Where(c => c.EarningId == earningId && c.CompanyId == companyId && c.IsActive)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }




        public async Task<bool> RestoreEarningAsync(Guid earningId, Guid companyId)
        {
            if (earningId == Guid.Empty)
            {
                throw new ArgumentException("Contract ID cannot be empty.", nameof(earningId));
            }

            if (companyId == Guid.Empty)
            {
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));
            }


            var earning = await _context.Earnings
                .Where(c => c.EarningId == earningId && c.CompanyId == companyId && !c.IsActive)
           .FirstOrDefaultAsync();

            if (earning == null)
            {
                return false;
            }

            earning.IsActive = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateEarningAsync(Earning earning)
        {
            if (earning == null)
                throw new ArgumentNullException(nameof(earning));

            var existingEarning = await _context.Earnings
                .FirstOrDefaultAsync(e => e.EarningId == earning.EarningId && e.CompanyId == earning.CompanyId && e.IsActive);

            if (existingEarning == null)
                return false;

            // 🔹 Update editable fields
            existingEarning.ContractId = earning.ContractId;
            existingEarning.SettlementId = earning.SettlementId;
            existingEarning.Platform = earning.Platform;
            existingEarning.GrossIncome = earning.GrossIncome;
            existingEarning.BtwPercentage = earning.BtwPercentage;
            existingEarning.IncomeDate = earning.IncomeDate;
            existingEarning.WeekStart = earning.WeekStart;
            existingEarning.WeekEnd = earning.WeekEnd;
            existingEarning.CompanyId = earning.CompanyId;

            // Optional — if you track update time
            existingEarning.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
