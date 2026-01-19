using CapManagement.Server.IService;
using CapManagement.Shared.DtoModels.CarDtoModels;
using CapManagement.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CapManagement.Shared.Models;
using Microsoft.AspNetCore.Authorization;

namespace CapManagement.Server.Controllers
{

    
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {

        private readonly ICarService _carService;

        private readonly RdwCarService _rdwCarService;
        public CarController(ICarService carService, RdwCarService rdwCarService)
        {
            _carService = carService;
            _rdwCarService = rdwCarService;
        }

        // POST: api/Car
        [HttpPost]
        public async Task<IActionResult> CreateCarAsync([FromBody] CreateCarDto carDto)
        {
            if (carDto == null)
            {
                return BadRequest(new ApiResponse<CarDto>
                {
                    Success = false,
                    Errors = new List<string> { "Car data is required." }
                });
            }

            // Replace Guid.NewGuid().ToString() with actual userId if available
            var response = await _carService.CreateCarAsync(carDto, Guid.NewGuid().ToString());

            if (!response.Success)
            {
                return BadRequest(response); // Already an ApiResponse<CarDto>
            }

            // Correct CreatedAtAction call pointing to GET by ID
            return CreatedAtAction(
                nameof(GetCarByIdAsync),                 // GET action for the car
                new { id = response.Data.CarId, companyId = response.Data.CompanyId },  // route + query params
                response
            );
        }

        // GET: api/Car
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<CarDto>>>> GetAllCarsAsync(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] Guid companyId = default,
    [FromQuery] string? orderBy = null,
    [FromQuery] string? filter = null)
        {
            var response = await _carService.GetAllCarAsync(pageNumber, pageSize, companyId, orderBy, filter);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CarDto>>> GetCarByIdAsync(Guid id, [FromQuery] Guid companyId)
        {
            var carDto = await _carService.GetCarByIdAsync(id, companyId);

            if (carDto == null)
                return NotFound(new ApiResponse<CarDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Car with ID {id} not found" }
                });

            return Ok(new ApiResponse<CarDto>
            {
                Success = true,
                Data = carDto
            });
        }

        // PUT: api/Car/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateCarAsync(Guid id, [FromBody] CarDto carDto)
        {
            if (id != carDto.CarId)
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { "Car ID in URL does not match the provided data." }
                });

            var result = await _carService.UpdateCarAsync(carDto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // PATCH: api/Car/{id}/archive
        [HttpPatch("{id}/archive")]
        public async Task<IActionResult> ArchiveCarAsync(Guid id, [FromQuery] Guid companyId)
        {
            var response = await _carService.ArchiveCarAsync(id, companyId);

            if (!response.Success)
                return NotFound(response);

            return Ok(new { Message = "Car archived successfully." });
        }

        // PATCH: api/Car/{id}/restore
        [HttpPatch("{carId}/restore")]
        public async Task<IActionResult> RestoreCarAsync(Guid carId, [FromQuery] Guid companyId)
        {
            try
            {
                var response = await _carService.RestoreCarAsync(carId, companyId);
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

        // GET: api/Car/archived
        [HttpGet("archived")]
        public async Task<IActionResult> GetArchivedCars(Guid companyId, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _carService.GetArchivedCarsAsync(pageNumber, pageSize, companyId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }


        [HttpGet("rdw/{numberPlate}")]
        public async Task<IActionResult> GetCarInfoFromRdw(string numberPlate)
        {
            var car = await _rdwCarService.GetCarInfoFromRdwAsync(numberPlate);
            return Ok(car);
        }

    }

}

