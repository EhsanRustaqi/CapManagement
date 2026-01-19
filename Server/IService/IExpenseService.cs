using CapManagement.Shared.DtoModels.EarningDtoModels;
using CapManagement.Shared.Models;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.ExpenseDtoModels;

namespace CapManagement.Server.IService
{
    public interface IExpenseService
    {


        Task<ApiResponse<PagedResponse<ExpenseDto>>> GetAllExpenseAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter);
        
        Task<ApiResponse<ExpenseDto>> CreateExpenseAsync(CreateExpenseDto expenseDto, string userId);



        Task<ExpenseDto> GetExpenseByIdAsync(Guid expenseId, Guid companyId);

        Task<ApiResponse<bool>> ArchiveExpenseAsync(Guid expenseId, Guid companyId);

        Task<ApiResponse<bool>> RestoreExpenseAsync(Guid expenseId, Guid companyId);

        Task<ApiResponse<bool>> UpdateExpenseAsync(UpdateExpenseDto UpdateExpenseDto);

        Task<ApiResponse<PagedResponse<ExpenseDto>>> GetArchivedExpensesAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter);



        Task<ApiResponse<PageResult<ExpenseDto>>> GetExpensesByCarIdAsync(
    Guid carId,
    Guid companyId,
    int pageNumber,
    int pageSize);

    }
}
