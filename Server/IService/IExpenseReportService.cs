using CapManagement.Shared;
using CapManagement.Shared.Models.Car_CompanyReportModels;

namespace CapManagement.Server.IService
{
    public interface IExpenseReportService
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
