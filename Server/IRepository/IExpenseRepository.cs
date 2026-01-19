using CapManagement.Shared.Models;

namespace CapManagement.Server.IRepository
{
    public interface IExpenseRepository
    {


        Task <PageResult<Expense>> GetAllExpensesAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter);

        Task<PageResult<Expense>> GetAllArchivedExpenseAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter);
        Task<Expense> CreateExpenseAsync(Expense expense);

        Task<bool> UpdateExpenseAsync(Expense expense);


        Task<bool> ArchiveExpenseAsync(Guid ExpenseId, Guid companyId);


        Task<bool> RestoreExpenseAsync(Guid expenseId, Guid companyId);


        Task<Expense?> GetExpenseByIdAsync(Guid expenseId, Guid companyId);

        // below is the new methods for generating report for each car



        Task<PageResult<Expense>> GetExpensesByCarIdAsync(
    Guid carId,
    Guid companyId,
    int pageNumber,
    int pageSize);

      // All expenses of a car in a period
        Task<List<Expense>> GetExpensesByCarAndPeriodAsync(
            Guid carId,
            Guid companyId,
            DateTime fromDate,
            DateTime toDate);

        // All expenses in a period (company-wide report)
        Task<List<Expense>> GetExpensesByPeriodAsync(
            Guid companyId,
            DateTime fromDate,
            DateTime toDate);

        // Car + period + type (Maintenance, Fuel, etc.)
        Task<List<Expense>> GetExpensesByCarPeriodAndTypeAsync(
            Guid carId,
            Guid companyId,
            DateTime fromDate,
            DateTime toDate,
            ExpenseType type);





    }
}
