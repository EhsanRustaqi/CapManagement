using CapManagement.Server.IService;
using CapManagement.Server.Services;
using CapManagement.Shared.DtoModels.CarDtoModels;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.ExpenseDtoModels;
using CapManagement.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using CapManagement.Shared.DtoModels.CompanyDtoModels;
using CapManagement.Shared.DtoModels.DriverDtoModels;
using Azure;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CapManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {

        private readonly IExpenseService _expenseService;
        public ExpenseController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }


        // GET: api/<ExpenseController>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<ExpenseDto>>>> GetAllExpenseAsync(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] Guid companyId = default,
    [FromQuery] string? orderBy = null,
    [FromQuery] string? filter = null)
        {
            var response = await _expenseService.GetAllExpenseAsync(pageNumber, pageSize, companyId, orderBy, filter);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    




       
        // POST api/<ExpenseController>
        [HttpPost]
        public async Task<IActionResult> CreateExpenseAsync([FromBody] CreateExpenseDto expenseDto)
        {
            if (expenseDto == null)
            {
                return BadRequest(new ApiResponse<ExpenseDto>
                {
                    Success = false,
                    Errors = new List<string> { "Expense data is required." }
                });
            }

            var response = await _expenseService.CreateExpenseAsync(expenseDto, Guid.NewGuid().ToString()); // replace with actual userId

            if (!response.Success)
                return BadRequest(response);

            // Always return 200 OK to the client
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ExpenseDto>>> GetExpenseByIdAsync(Guid id, [FromQuery] Guid companyId)
        {
            var expenseDto = await _expenseService.GetExpenseByIdAsync(id, companyId);

            if (expenseDto == null)
                return NotFound(new ApiResponse<ExpenseDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Car with ID {id} not found" }
                });

            return Ok(new ApiResponse<ExpenseDto>
            {
                Success = true,
                Data = expenseDto
            });
        }

        // PUT: api/Car/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateExpenseAsync(Guid id, [FromBody] UpdateExpenseDto expenseDto)
        {
            if (id != expenseDto.ExpenseId)
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { "expense ID in URL does not match the provided data." }
                });

            var result = await _expenseService.UpdateExpenseAsync(expenseDto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPatch("{id}/archive")]
        public async Task<IActionResult> ArchiveExpenseAsync(Guid id, [FromQuery] Guid companyId)
        {
            var response = await _expenseService.ArchiveExpenseAsync(id, companyId);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        // PATCH: api/Car/{id}/restore
        [HttpPatch("{carId}/restore")]
        public async Task<IActionResult> RestoreExpenseAsync(Guid carId, [FromQuery] Guid companyId)
        {
            try
            {
                var response = await _expenseService.RestoreExpenseAsync(carId, companyId);
                if (!response.Success)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { $"Failed to restore expense: {ex.Message}" }
                });
            }
        }

        // GET: api/Car/archived
        [HttpGet("archived")]
        public async Task<IActionResult> GetArchivedExpensesAsync(
    [FromQuery] Guid companyId,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? orderBy = null,
    [FromQuery] string? filter = null)
        {
            var result = await _expenseService.GetArchivedExpensesAsync(pageNumber, pageSize, companyId, orderBy, filter);
            if (!result.Success)
            {

                if (!result.Success)
                    return BadRequest(result);  // More descriptive; adjust if result has Error prop
            }
            return Ok(result);
        }




        [HttpGet("by-car/{carId}")]
     public async Task<ActionResult<ApiResponse<PageResult<ExpenseDto>>>> GetByCarId(
    Guid carId,
    Guid companyId,
    int pageNumber = 1,
    int pageSize = 10)
        {
            var result = await _expenseService.GetExpensesByCarIdAsync(
                carId,
                companyId,
                pageNumber,
                pageSize);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }



    }




}
