using CapManagement.Server.IService;
using CapManagement.Server.Services;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.CarDtoModels;
using CapManagement.Shared.DtoModels.EarningDtoModels;
using CapManagement.Shared.DtoModels.SettlementDtoModels;
using CapManagement.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CapManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EarningController : ControllerBase
    {
        private readonly IEarningService _earningService;
        public EarningController(IEarningService earningService)
        {
            _earningService = earningService;
        }


        [HttpPost]
        public async Task<IActionResult> CreateEarningAsync([FromBody] CreateEarningDto earningDto)

        {
            if (earningDto == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Errors = new List<string> { "Earning data cannot be null." }
                });
            }

            try
            {
                var userId = User?.Identity?.Name ?? "system";

                var result = await _earningService.CreateEarningAsync(earningDto, userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Earning successfully created."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while creating earning.", ex.Message }
                });
            }
        }



        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<EarningDto>>>> GetAllEarningAsync(
         [FromQuery] int pageNumber = 1,
         [FromQuery] int pageSize = 10,
         [FromQuery] Guid companyId = default,
         [FromQuery] string? orderBy = null,
         [FromQuery] string? filter = null)
        {
            var response = await _earningService.GetAllEarningAsync(pageNumber, pageSize, companyId, orderBy, filter);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }



        [HttpPost("create-batch")]
        public async Task<ActionResult<ApiResponse<List<EarningDto>>>> CreateBatch([FromBody] List<CreateEarningDto> earnings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<List<EarningDto>> { Success = false, Message = "Invalid model state." });
            }

            if (!earnings.Any())
            {
                return BadRequest(new ApiResponse<List<EarningDto>> { Success = false, Message = "No earnings provided." });
            }

            var response = await _earningService.CreateEarningsAsync(earnings);
            if (response.Success)
            {
                return Ok(response);  // 200 with { success: true, data: [saved earnings with IDs] }
            }

            return BadRequest(response);  // 400 with error message
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<EarningDto>>> GetEarningByIdAsync(Guid id, [FromQuery] Guid companyId)
        {
            var earningDto = await _earningService.GetEarningByIdAsync(id, companyId);

            if (earningDto == null)
                return NotFound(new ApiResponse<EarningDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Driver with ID {id} not found" }
                });

            return Ok(new ApiResponse<EarningDto>
            {
                Success = true,
                Data = earningDto
            });
        }


    }
}
