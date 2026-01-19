using CapManagement.Shared.DtoModels.CarDtoModels;
using CapManagement.Shared.DtoModels.ExpenseDtoModels;
using CapManagement.Shared.Models;
using CapManagement.Shared;

namespace CapManagement.Client.IServiceClient
{
    public interface IExpenseServiceClient
    {
        Task<ApiResponse<PagedResponse<ExpenseDto>>> GetExpenseAsync(Guid companyId, int pageNumber = 1, int pageSize = 10, string? orderBy = null, string? filter = null);
        Task<ApiResponse<ExpenseDto>> CreateExpenseAsync(CreateExpenseDto dto);

        Task<ApiResponse<bool>> ArchiveExpenseAsync(Guid expenseId, Guid companyId);

        Task<ApiResponse<bool>> RestoreExpenseAsync(Guid expenseId, Guid companyId);

        Task<ApiResponse<PagedResponse<ExpenseDto>>> GetArchivedExpenseAsync(Guid companyId, int pageNumber = 1, int pageSize = 10);

        Task<ApiResponse<ExpenseDto>> GetExpenseByIdAsync(Guid expenseId, Guid companyId);

        Task<ApiResponse<bool>> UpdateExpenseAsync(UpdateExpenseDto expense);


        // below is the method for expense report component
        Task<ApiResponse<PageResult<ExpenseDto>>> GetExpensesByCarIdAsync(
       Guid carId,
       Guid companyId,
       int pageNumber,
       int pageSize);
    }
}
