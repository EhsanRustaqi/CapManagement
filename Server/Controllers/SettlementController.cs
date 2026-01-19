using CapManagement.Server.IService;
using CapManagement.Shared.DtoModels.SettlementDtoModels;
using CapManagement.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CapManagement.Server.Services;
using CapManagement.Shared.DtoModels.EarningDtoModels;
using CapManagement.Shared.Models;
using CapManagement.Shared.DtoModels.DriverDtoModels;

namespace CapManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettlementController : ControllerBase
    {
        private readonly ISettlmentService _settlementService;
         
        private readonly PdfService _pdfService;
        public SettlementController(ISettlmentService settlementService, PdfService pdfService)
        {
            _settlementService = settlementService;
            _pdfService = pdfService;
        }
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<SettlementDto>>> Create([FromBody] CreateSettlementDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<SettlementDto> { Success = false, Message = "Invalid model state." });
            }

            var response = await _settlementService.CreateSettlementAsync(dto);
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }



        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<SettlementDto>>>> GetAllSettlmentAsync(
      [FromQuery] int pageNumber = 1,
      [FromQuery] int pageSize = 10,
      [FromQuery] Guid companyId = default,
        [FromQuery] string? orderBy = null,
      [FromQuery] string? filter = null)
        {
            var response = await _settlementService.GetAllSettlementAsync(pageNumber, pageSize, companyId, orderBy, filter);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }




        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SettlementDto>>> GetSettlmentByIdAsync(Guid id, [FromQuery] Guid companyId)
        {
            var settlementDto = await _settlementService.GetSettlementByIdAsync(id, companyId);

            if (settlementDto == null)
                return NotFound(new ApiResponse<SettlementDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Driver with ID {id} not found" }
                });

            return Ok(new ApiResponse<SettlementDto>
            {
                Success = true,
                Data = settlementDto
            });
        }




        [HttpGet("{id}/{companyId}/pdf")]
        public async Task<IActionResult> GetSettlementPdf(Guid id, Guid companyId)
        {
            var settlement = await _settlementService.GetSettlementByIdAsync(id, companyId);

            if (settlement == null)
                return NotFound();

            var pdf = _pdfService.GenerateSettlmentPdf(settlement);

            return File(pdf, "application/pdf", $"Settlement_{id}.pdf");
        }




    }
}
