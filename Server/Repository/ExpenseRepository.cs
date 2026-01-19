using CapManagement.Client.Pages.Driver.Driver;
using CapManagement.Server.DbContexts;
using CapManagement.Server.IRepository;
using CapManagement.Shared.DtoModels.ExpenseDtoModels;
using CapManagement.Shared.Models;
using Microsoft.EntityFrameworkCore;

using Microsoft.IdentityModel.Tokens;
using System.Linq.Dynamic.Core;
namespace CapManagement.Server.Repository
{
    public class ExpenseRepository : IExpenseRepository
    {

        private readonly FleetDbContext _context;
        public ExpenseRepository(FleetDbContext context)
        {
            _context = context;
        }





        public async Task<bool> ArchiveExpenseAsync(Guid ExpenseId, Guid companyId)
        {
            var expense = await _context.Expenses
                 .FirstOrDefaultAsync(e => e.ExpenseId == ExpenseId && e.CompanyId == companyId);

            if (expense == null)
                return false;

            expense.IsActive = false;

            expense.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;

        }






        public async Task<Expense> CreateExpenseAsync(Expense expense)
        {
            _context.Expenses.Add(expense);

            await _context.SaveChangesAsync();
            return expense;
        }






        public async Task<PageResult<Expense>> GetAllArchivedExpenseAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));
            }

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, Math.Min(pageSize, 100));

            var query = _context.Expenses
                .Include(c => c.Car)
                .Where(c => c.CompanyId == companyId && !c.IsActive) // archived contracts
                .AsNoTracking();

            // Include Car and Driver if filter/orderBy references them
            if (!string.IsNullOrEmpty(filter) && (filter.Contains("Expense.") || filter.Contains("Expense.")))
            {
                query = query.Include(c => c.Car);
            }
            else if (!string.IsNullOrEmpty(orderBy) && (orderBy.Contains("Car.")))
            {
                query = query.Include(c => c.Car);
            }

            // Apply filtering
            if (!string.IsNullOrEmpty(filter))
            {
                try
                {
                    query = query.Where(filter); // requires System.Linq.Dynamic.Core
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Filter error: {ex.Message}");
                    return new PageResult<Expense>
                    {
                        Items = new List<Expense>(),
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
                    query = query.OrderBy(orderBy); // requires System.Linq.Dynamic.Core
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Sort error: {ex.Message}");
                    query = query.OrderBy(c => c.ExpenseId);
                }
            }
            else
            {
                query = query.OrderBy(c => c.ExpenseId);
            }

            var totalCount = await query.CountAsync();
            var expenses = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageResult<Expense>
            {
                Items = expenses,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    






        public async Task<PageResult<Expense>> GetAllExpensesAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));
            }

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, Math.Min(pageSize, 100));


            var query = _context.Expenses
                .Include(e => e.Car)
                .Where(e => e.CompanyId == companyId && e.IsActive)
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
                    return new PageResult<Expense>
                    {
                        Items = new List<Expense>(),
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
                    query = query.OrderBy(c => c.ExpenseId); // Default sort
                }
            }
            else
            {
                query = query.OrderBy(c => c.ExpenseId); // Default sort by CarId
            }

         
            var totalCount = await query.CountAsync();
            var expenses = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageResult<Expense>
            {
                Items = expenses,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };



        }






            public async Task<Expense?> GetExpenseByIdAsync(Guid expenseId, Guid companyId)
            {
            if (expenseId == Guid.Empty)
                throw new ArgumentException("Driver ID cannot be empty.", nameof(expenseId));

            if (companyId == Guid.Empty)
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

            return await _context.Expenses
                .Include(d => d.Car)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.ExpenseId == expenseId
                                       && d.CompanyId == companyId
                                       && d.IsActive);



            }






        public async Task<List<Expense>> GetExpensesByCarAndPeriodAsync(Guid carId, Guid companyId, DateTime fromDate, DateTime toDate)
        {
            if (carId == Guid.Empty)
                throw new ArgumentException("Car ID cannot be empty.", nameof(carId));

            if (companyId == Guid.Empty)
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

            if (fromDate > toDate)
                throw new ArgumentException("FromDate cannot be greater than ToDate.");

            return await _context.Expenses
                .AsNoTracking()
                .Include(e => e.Car)
                .Where(e =>
                    e.CarId == carId &&
                    e.CompanyId == companyId &&
                    e.IsActive &&
                    e.ExpenseDate >= fromDate &&
                    e.ExpenseDate <= toDate)
                .OrderBy(e => e.ExpenseDate)
                .ToListAsync();
        }

        public async Task<PageResult<Expense>> GetExpensesByCarIdAsync(Guid carId, Guid companyId, int pageNumber, int pageSize)
        {
            if (carId == Guid.Empty)
                throw new ArgumentException("Car ID cannot be empty", nameof(carId));

            if (companyId == Guid.Empty)
                throw new ArgumentException("Company ID cannot be empty", nameof(companyId));

            var query = _context.Expenses
                .AsNoTracking()
                .Include(e => e.Car)
                .Where(e =>
                    e.CarId == carId &&
                    e.CompanyId == companyId &&
                    e.IsActive);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(e => e.ExpenseDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageResult<Expense>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<List<Expense>> GetExpensesByCarPeriodAndTypeAsync(Guid carId, Guid companyId, DateTime fromDate, DateTime toDate, ExpenseType type)
        {
            if (carId == Guid.Empty)
                throw new ArgumentException("Car ID cannot be empty.", nameof(carId));

            if (companyId == Guid.Empty)
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

            return await _context.Expenses
                .AsNoTracking()
                .Include(e => e.Car)
                .Where(e =>
                    e.CarId == carId &&
                    e.CompanyId == companyId &&
                    e.IsActive &&
                    e.Type == type &&
                    e.ExpenseDate >= fromDate &&
                    e.ExpenseDate <= toDate)
                .OrderBy(e => e.ExpenseDate)
                .ToListAsync();
        }




        public async Task<List<Expense>> GetExpensesByPeriodAsync(Guid companyId, DateTime fromDate, DateTime toDate)
        {
            if (companyId == Guid.Empty)
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

            return await _context.Expenses
                .AsNoTracking()
                .Include(e => e.Car)
                .Where(e =>
                    e.CompanyId == companyId &&
                    e.IsActive &&
                    e.ExpenseDate >= fromDate &&
                    e.ExpenseDate <= toDate)
                .OrderBy(e => e.ExpenseDate)
                .ToListAsync();
        }





        public async Task<bool> RestoreExpenseAsync(Guid expenseId, Guid companyId)
            {
            var expense = await _context.Expenses
         .IgnoreQueryFilters() // allow fetching archived/inactive
           .FirstOrDefaultAsync(d => d.ExpenseId == expenseId && d.CompanyId == companyId);

            if (expense == null)
            {
                Console.WriteLine($"No expense found with ID {expenseId}");
                return false;
            }

            Console.WriteLine($"Restoring company {expense.CompanyId}, WasActive: {expense.IsActive}, DeletedAt: {expense.DeletedAt}");


            // Restore flags
            expense.IsActive = true;
            expense.IsDeleted = false;    // ✅ ensure global filters pick it up again
            expense.DeletedAt = null;
            expense.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"Restored company {expense.ExpenseId}, IsActive: {expense.IsActive}, IsDeleted: {expense.IsDeleted}, DeletedAt: {expense.DeletedAt}");
                return true;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error restoring company: {ex.InnerException?.Message ?? ex.Message}");
                return false;
            }

        }




            public async Task<bool> UpdateExpenseAsync(Expense expense)
            {
            if (expense == null) throw new ArgumentNullException(nameof(expense));

            var existingExpense = await _context.Expenses
                .FirstOrDefaultAsync(c => c.ExpenseId == expense.ExpenseId && c.CompanyId == expense.CompanyId && c.IsActive);

            if (existingExpense == null) return false;

            // Update fields (no business validation here)
            existingExpense.CarId = expense.CarId;
            existingExpense.Type = expense.Type;
            existingExpense.Amount = expense.Amount;
            existingExpense.VatPercent = expense.VatPercent;
            
         
            existingExpense.ExpenseDate = expense.ExpenseDate;
           

            await _context.SaveChangesAsync();
            return true;
        }
        }
    }



