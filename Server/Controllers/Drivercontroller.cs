using CapManagement.Server.IService;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.DriverDtoModels;
using CapManagement.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace CapManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _driverService;

        public DriverController(IDriverService driverService)
        {
            _driverService = driverService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDriverAsync([FromBody] CreateDriverDto driverDto)
        {
            if (driverDto == null)
            {
                return BadRequest(new ApiResponse<DriverDto>
                {
                    Success = false,
                    Errors = new List<string> { "Driver data is required." }
                });
            }

            var response = await _driverService.CreateDriverAsync(driverDto, Guid.NewGuid().ToString()); // replace with actual userId

            if (!response.Success)
            
                return BadRequest(response); // response is already ApiResponse<DriverDto>
                
            return Ok(response);
            

           
        }


        // GET: api/Driver
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<DriverDto>>>> GetAllDriversAsync(
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] Guid companyId = default)
        {
            var response = await _driverService.GetAllDriverAsync(pageNumber, pageSize, companyId);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        // GET: api/Driver/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DriverDto>>> GetDriverByIdAsync(Guid id, [FromQuery] Guid companyId)
        {
            var driverDto = await _driverService.GetDriverByIdAsync(id, companyId);

            if (driverDto == null)
                return NotFound(new ApiResponse<DriverDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Driver with ID {id} not found" }
                });

            return Ok(new ApiResponse<DriverDto>
            {
                Success = true,
                Data = driverDto
            });
        }

        // PUT: api/Driver/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateDriverAsync(Guid id, [FromBody] DriverDto driverDto)
        {
            if (id != driverDto.DriverId)
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { "Driver ID in URL does not match the provided data." }
                });

            var result = await _driverService.UpdateDriverAsync(driverDto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // PATCH: api/Driver/{id}/archive
        [HttpPatch("{id}/archive")]
        public async Task<IActionResult> ArchiveDriverAsync(Guid id, [FromQuery] Guid companyId)
        {
            var response = await _driverService.ArchiveDriverAsync(id, companyId);

            if (!response.Success)
                return NotFound(response);

            return Ok(new { Message = "Driver archived successfully." });
        }

        // PATCH: api/Driver/{id}/restore
        [HttpPatch("{driverId}/restore")]
        public async Task<IActionResult> RestoreDriverAsync(Guid driverId, [FromQuery] Guid companyId)
        {
            try
            {
                var response = await _driverService.RestoreDriverAsync(driverId, companyId);
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
                    Errors = new List<string> { $"Failed to restore driver: {ex.Message}" }
                });
            }
        }





        [HttpGet("archived")]
      
        public async Task<IActionResult> GetArchivedDrivers(Guid companyId, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _driverService.GetArchivedDriversAsync(pageNumber, pageSize, companyId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }


        // GET: api/Driver/search
        //[HttpGet("search")]
        //public async Task<ActionResult<ApiResponse<DriverDto>>> GetDriverByNameAsync(
        //    [FromQuery] string firstName, [FromQuery] string lastName, [FromQuery] Guid companyId)
        //{
        //    var driverDto = await _driverService.GetDriverByNameAsync(firstName, lastName, companyId);

        //    if (driverDto == null)
        //        return NotFound(new ApiResponse<DriverDto>
        //        {
        //            Success = false,
        //            Errors = new List<string> { $"Driver {firstName} {lastName} not found" }
        //        });

        //    return Ok(new ApiResponse<DriverDto>
        //    {
        //        Success = true,
        //        Data = driverDto
        //    });
        //}
    }
}
