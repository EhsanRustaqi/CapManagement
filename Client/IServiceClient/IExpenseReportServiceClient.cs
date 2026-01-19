using CapManagement.Shared.Models.Car_CompanyReportModels;
using CapManagement.Shared;

namespace CapManagement.Client.IServiceClient
{
    public interface IExpenseReportServiceClient
    {
        Task<ApiResponse<ExpenseReportSummaryDto>> GetCarExpenseReportAsync(
   Guid carId,
   Guid companyId,
   DateTime fromDate,
   DateTime toDate);

        Task<ApiResponse<ExpenseReportSummaryDto>> GetCompanyExpenseReportAsync(
            Guid companyId,
            DateTime fromDate,
            DateTime toDate);
    }
}
