using CapManagement.Server.IService;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.CompanyDtoModels;
using CapManagement.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CapManagment.Server.Controllers
{


    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
 
    public class CompanyController : ControllerBase
    {

        private readonly ICompanyService _companyService;
        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }


        /// <summary>
        /// Create a new company
        /// </summary>
        /// <param name="companyDto">Company creation DTO</param>
        /// <returns>Created company or errors</returns>
        [HttpPost]
        //  public async Task<IActionResult> CreateCompanyAsync(
        //[FromBody] CreateCompanyDto companyDto,
        //[FromQuery] string userId)
        //  {
        //      if (companyDto == null)
        //          return BadRequest(new { Errors = new[] { "Company data is required." } });

        //      if (string.IsNullOrWhiteSpace(companyDto.CompanyName))
        //          return BadRequest(new { Errors = new[] { "Company name is required." } });

        //      if (string.IsNullOrWhiteSpace(userId))
        //          return BadRequest(new { Errors = new[] { "User ID is required." } });

        //      var response = await _companyService.CreateCompanyAsync(companyDto, userId);

        //      if (!response.Success)
        //          return BadRequest(new { Errors = response.Errors });

        //      return Created("", response.Data);
        //  }



        [HttpPost]
        public async Task<IActionResult> CreateCompanyAsync([FromBody] CreateCompanyDto companyDto)
        {
            if (companyDto == null)
                return BadRequest(new { Errors = new[] { "Company data is required." } });

            if (string.IsNullOrWhiteSpace(companyDto.CompanyName))
                return BadRequest(new { Errors = new[] { "Company name is required." } });

            // TODO: Replace userId with actual user context later
            var userId = Guid.NewGuid().ToString();

            var response = await _companyService.CreateCompanyAsync(companyDto, userId);

            if (!response.Success)
                return BadRequest(new { Errors = response.Errors });

            return Created("", response.Data);
        }



        /// <summary>
        /// Get all companies with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default 1)</param>
        /// <param name="pageSize">Page size (default 10)</param>
        /// <returns>Paged list of companies</returns>
        //[HttpGet]
        //public async Task<IActionResult> GetAllCompaniesAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        //{
        //    try
        //    {
        //        // Call service to get paged companies
        //        var response = await _companyService.GetAllCompanyAsync(pageNumber, pageSize);

        //        if (!response.Success)
        //        {
        //            // Return 400 Bad Request with errors
        //            return BadRequest(new { response.Errors });
        //        }

        //        // Always return JSON with proper PagedResponse
        //        return Ok(new
        //        {
        //            Data = response.Data.Data,
        //            PageNumber = response.Data.PageNumber,
        //            PageSize = response.Data.PageSize,
        //            TotalPages = response.Data.TotalPages,
        //            TotalItems = response.Data.TotalItems,
        //            HasPreviousPage = response.Data.HasPreviousPage,
        //            HasNextPage = response.Data.HasNextPage
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Return 500 Internal Server Error with exception message
        //        return StatusCode(500, new
        //        {
        //            Errors = new[] { "An error occurred while fetching companies.", ex.Message }
        //        });
        //    }
        //}

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<CompanyDto>>>> GetAllCompaniesAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var response = await _companyService.GetAllCompanyAsync(pageNumber, pageSize);
                if (!response.Success)
                {
                    return BadRequest(response); // Return ApiResponse with errors
                }
                return Ok(response); // Return full ApiResponse<PagedResponse<CompanyDto>>
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PagedResponse<CompanyDto>>
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while fetching companies.", ex.Message }
                });
            }
        }

        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetCompanyById(Guid id)
        //{
        //    var companyDto = await _companyService.GetCompanyByIdAsync(id);

        //    if (companyDto == null)
        //        return NotFound();

        //    return Ok(companyDto);
        //}



        //[HttpDelete("{id}")]

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CompanyDto>>> GetCompanyByIdAsync(Guid id)
        {
            try
            {
                var companyDto = await _companyService.GetCompanyByIdAsync(id);
                if (companyDto == null)
                {
                    return NotFound(new ApiResponse<CompanyDto>
                    {
                        Success = false,
                        Errors = new List<string> { $"Company with ID {id} not found" }
                    });
                }
                return Ok(new ApiResponse<CompanyDto>
                {
                    Success = true,
                    Data = companyDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CompanyDto>
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while fetching the company.", ex.Message }
                });
            }
        }



        [HttpPatch("{id}/archive")]
        public async Task<IActionResult> ArchiveCompany(Guid id)
        {
            var response = await _companyService.ArchiveCompanyAsync(id);

            if (!response.Success)
            {
                return NotFound(new { Errors = response.Errors });
            }

            return Ok(new { Message = "Company archived successfully." });
        }


        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> RestoreCompany(Guid id)
        {
            var response = await _companyService.RestoreCompanyAsync(id);

            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);

           
        }

        [HttpGet("archived")]
        public async Task<IActionResult> GetArchivedCompanies(int pageNumber = 1, int pageSize = 10)
        {
            var response = await _companyService.GetArchivedCompaniesAsync(pageNumber, pageSize);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }



        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateCompanyAsync(Guid id, [FromBody] CompanyDto companyDto)
        {
            try
            {
                if (id != companyDto.CompanyId)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Errors = new List<string> { "Company ID in URL does not match the provided data." }
                    });
                }
                var result = await _companyService.UpdateCompanyAsync(companyDto);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while updating the company.", ex.Message }
                });
            }
        }
    }
}
