using CapManagement.Server.IService;
using CapManagement.Server.Services;
using CapManagement.Shared.DtoModels.CarDtoModels;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.ContractDto;
using CapManagement.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CapManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractController : ControllerBase
    {

        private readonly IContractService _contractService;
        public ContractController(IContractService contractService)
        {
            _contractService = contractService;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateContract([FromForm] CreateContractRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Convert IFormFile → byte[]
            byte[]? pdfBytes = null;
            if (request.PdfFile != null)
            {
                using var ms = new MemoryStream();
                await request.PdfFile.CopyToAsync(ms);
                pdfBytes = ms.ToArray();
            }

            // Map request → service DTO
            var dto = new CreateContractDto
            {
                CompanyId = request.CompanyId,
                DriverId = request.DriverId,
                CarId = request.CarId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = Enum.TryParse<ContractStatus>(request.Status, true, out var parsedStatus)
                            ? parsedStatus
                            : ContractStatus.Pending,
                PaymentAmount = request.PaymentAmount,
                Description = request.Description,
                Conditions = request.Conditions,
                PdfContent = pdfBytes
            };

            // Pass to service
            var userId = User?.Identity?.Name ?? "system"; // or from JWT
            var response = await _contractService.CreateContractAsync(dto, userId);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }


        // GET: api/Contract
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<ContractDto>>>> GetAllContractsAsync(
       [FromQuery] int pageNumber = 1,
       [FromQuery] int pageSize = 10,
        [FromQuery] Guid companyId = default,
        [FromQuery] string? orderBy = null,
        [FromQuery] string? filter = null)
        {
            var response = await _contractService.GetAllContractAsync(pageNumber, pageSize, companyId, orderBy, filter);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }


        [HttpGet("{contractId}/pdf")]
        public async Task<IActionResult> GetContractPdfAsync(Guid contractId, [FromQuery] Guid companyId)
        {
            try
            {
                if (contractId == Guid.Empty || companyId == Guid.Empty)
                {

                    return BadRequest(new ApiResponse<byte[]>
                    {
                        Success = false,
                        Errors = new List<string> { "Invalid contract or company ID." }
                    });
                }

                var response = await _contractService.GetContractPdfAsync(contractId, companyId);
                if (!response.Success)
                {

                    return BadRequest(response);
                }

                // Return PDF as a file
                return File(response.Data, "application/pdf", $"contract-{contractId}.pdf");
            }
            catch (Exception ex)
            {

                return StatusCode(500, new ApiResponse<byte[]>
                {
                    Success = false,
                    Errors = new List<string> { $"Unexpected error: {ex.Message}" }
                });
            }
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ContractDto>>> GetContractByIdAsync(Guid id, [FromQuery] Guid companyId)
        {
            var contractDto = await _contractService.GetContractByIdAsync(id, companyId);

            if (contractDto == null)
                return NotFound(new ApiResponse<ContractDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Car with ID {id} not found" }
                });

            return Ok(new ApiResponse<ContractDto>
            {
                Success = true,
                Data = contractDto
            });
        }


        // PUT: api/Car/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateContractAsync(Guid id, [FromBody] UpdateContractDto updateContractDto)
        {
            if (id != updateContractDto.ContractId)
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { "Contract ID in URL does not match the provided data." }
                });

            var result = await _contractService.UpdateContractAsync(updateContractDto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }





        // PATCH: api/Contract/{id}/archive
        [HttpPatch("{id}/archive")]
        public async Task<IActionResult> ArchiveContractAsync(Guid id, [FromQuery] Guid companyId)
        {
            var response = await _contractService.ArchiveContractAsync(id, companyId);

            if (!response.Success)
                return NotFound(response);

            return Ok(new { Message = "contract archived successfully." });
        }
        // inactive contract list
        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<PagedResponse<ContractDto>>>> GetInactiveContractsAsync(
     [FromQuery] int pageNumber = 1,
     [FromQuery] int pageSize = 10,
     [FromQuery] Guid companyId = default,
     [FromQuery] string? orderBy = null,
     [FromQuery] string? filter = null)
        {
            var response = await _contractService.GetAllInActiveContractAsync(pageNumber, pageSize, companyId, orderBy, filter);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }




        // PATCH: api/Car/{id}/restore
        [HttpPatch("{contractId}/restore")]
        public async Task<IActionResult> RestoreContractAsync(Guid contractId, [FromQuery] Guid companyId)
        {
            try
            {
                var response = await _contractService.RestoreContractAsync(contractId, companyId);
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
                    Errors = new List<string> { $"Failed to restore car: {ex.Message}" }
                });
            }
        }



        // ✅ Get archived contracts (paged)
        [HttpGet("archived")]
        public async Task<ActionResult<ApiResponse<PagedResponse<ContractDto>>>>
            GetArchivedContractsAsync(
                int pageNumber = 1,
                int pageSize = 10,
                Guid? companyId = null,
                string? orderBy = null,
                string? filter = null)
        {
            // ⚠️ Hardcode companyId for now until you add authentication
            var effectiveCompanyId = companyId ?? Guid.Parse("9D176E43-E0FF-4755-B130-625189F3991B");

            var result = await _contractService.GetArchivedContractsAsync(
                pageNumber,
                pageSize,
                effectiveCompanyId,
                orderBy,
                filter
            );

            return Ok(result);
        }




    


    }





}
