using CapManagement.Server.IService;
using CapManagement.Shared.Models.Car_CompanyReportModels;
using CapManagement.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CapManagement.Server.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseReportController : ControllerBase
    {
        private readonly IExpenseReportService _expenseReportService;


        public ExpenseReportController(IExpenseReportService expenseReportService)
        {
            _expenseReportService = expenseReportService;
        }

        [HttpGet("report/car")]
        public async Task<ActionResult<ApiResponse<ExpenseReportSummaryDto>>> GetCarReport(
       Guid carId,
       Guid companyId,
       DateTime fromDate,
       DateTime toDate)
        {
            var result = await _expenseReportService
                .GetCarExpenseReportAsync(carId, companyId, fromDate, toDate);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("company")]
        public async Task<ActionResult<ApiResponse<ExpenseReportSummaryDto>>> GetCompanyReport(
            Guid companyId,
            DateTime fromDate,
            DateTime toDate)
        {
            var result = await _expenseReportService
                .GetCompanyExpenseReportAsync(companyId, fromDate, toDate);

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
