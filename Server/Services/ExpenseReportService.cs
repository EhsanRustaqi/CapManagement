using CapManagement.Server.IRepository;
using CapManagement.Server.IService;
using CapManagement.Shared;
using CapManagement.Shared.Models.Car_CompanyReportModels;

namespace CapManagement.Server.Services
{
    public class ExpenseReportService : IExpenseReportService
    {
        private readonly IExpenseRepository _expenseRepository;
        public ExpenseReportService(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }



        /// <summary>
        /// Generates a summarized expense report for a specific car within a given date range.
        /// </summary>
        /// <param name="carId">
        /// The unique identifier of the car for which the expense report is generated.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company that owns the car.
        /// </param>
        /// <param name="fromDate">
        /// The start date of the reporting period (inclusive).
        /// </param>
        /// <param name="toDate">
        /// The end date of the reporting period (inclusive).
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{ExpenseReportSummaryDto}"/> containing the aggregated expense data,
        /// grouped by expense type, including total net, VAT, and gross amounts.
        /// </returns>
        /// <remarks>
        /// This method validates the input IDs, retrieves all expenses for the specified car and period,
        /// aggregates them by expense type, and returns a financial summary suitable for reporting and VAT analysis.
        /// If no expenses are found, an empty report is returned with the specified date range.
        /// </remarks>


        public async Task<ApiResponse<ExpenseReportSummaryDto>> GetCarExpenseReportAsync(Guid carId, Guid companyId, DateTime fromDate, DateTime toDate)
        {
            var response = new ApiResponse<ExpenseReportSummaryDto>();

            if (carId == Guid.Empty || companyId == Guid.Empty)
            {
                response.Success = false;
                response.Message = "Invalid car or company ID.";
                return response;
            }

            try
            {
                
                var expenses = await _expenseRepository
                    .GetExpensesByCarAndPeriodAsync(carId, companyId, fromDate, toDate);

                if (expenses == null || !expenses.Any())
                {
                    response.Success = true;
                    response.Data = new ExpenseReportSummaryDto
                    {
                        CarId = carId,
                        CompanyId = companyId,
                        FromDate = fromDate,
                        ToDate = toDate
                    };
                    return response;
                }

                var report = new ExpenseReportSummaryDto
                {
                    CarId = carId,
                    CompanyId = companyId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    TotalNetAmount = expenses.Sum(e => e.NetAmount),
                    TotalVatAmount = expenses.Sum(e => e.VatAmount),
                    TotalGrossAmount = expenses.Sum(e => e.Amount),
                    ByType = expenses
                        .GroupBy(e => e.Type)
                        .Select(g => new ExpenseReportItemDto
                        {
                            Type = g.Key,
                            TotalNetAmount = g.Sum(x => x.NetAmount),
                            TotalVatAmount = g.Sum(x => x.VatAmount),
                            TotalGrossAmount = g.Sum(x => x.Amount)
                        })
                        .ToList()
                };

                response.Success = true;
                response.Data = report;
                response.Message = "Car expense report generated successfully.";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error generating car expense report: {ex.Message}";
                return response;
            }
        }





        /// <summary>
        /// Generates a summarized expense report for a company within a specified date range.
        /// </summary>
        /// <param name="companyId">
        /// The unique identifier of the company for which the expense report is generated.
        /// </param>
        /// <param name="fromDate">
        /// The start date of the reporting period (inclusive).
        /// </param>
        /// <param name="toDate">
        /// The end date of the reporting period (inclusive).
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{ExpenseReportSummaryDto}"/> containing aggregated expense data,
        /// grouped by expense type, including total net, VAT, and gross amounts.
        /// </returns>
        /// <remarks>
        /// This method retrieves all company-level expenses within the specified period,
        /// calculates financial totals, groups them by expense type, and returns a summary report
        /// suitable for financial analysis and VAT reporting.
        /// If no expenses are found, an empty report is returned with the specified date range.
        /// </remarks>
        public async Task<ApiResponse<ExpenseReportSummaryDto>> GetCompanyExpenseReportAsync(Guid companyId, DateTime fromDate, DateTime toDate)
        {
            var response = new ApiResponse<ExpenseReportSummaryDto>();

            if (companyId == Guid.Empty)
            {
                response.Success = false;
                response.Message = "Company ID is required.";
                return response;
            }

            try
            {
                var expenses = await _expenseRepository
                    .GetExpensesByPeriodAsync(companyId, fromDate, toDate);

                if (expenses == null || !expenses.Any())
                {
                    response.Success = true;
                    response.Data = new ExpenseReportSummaryDto
                    {
                        CompanyId = companyId,
                        FromDate = fromDate,
                        ToDate = toDate
                    };
                    return response;
                }

                var report = new ExpenseReportSummaryDto
                {
                    CompanyId = companyId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    TotalNetAmount = expenses.Sum(e => e.NetAmount),
                    TotalVatAmount = expenses.Sum(e => e.VatAmount),
                    TotalGrossAmount = expenses.Sum(e => e.Amount),
                    ByType = expenses
                        .GroupBy(e => e.Type)
                        .Select(g => new ExpenseReportItemDto
                        {
                            Type = g.Key,
                            TotalNetAmount = g.Sum(x => x.NetAmount),
                            TotalVatAmount = g.Sum(x => x.VatAmount),
                            TotalGrossAmount = g.Sum(x => x.Amount)
                        })
                        .ToList()
                };

                response.Success = true;
                response.Data = report;
                response.Message = "Company expense report generated successfully.";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error generating company expense report: {ex.Message}";
                return response;
            }
        }
    }
}
