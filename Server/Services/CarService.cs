using CapManagement.Client.Pages.Driver.Driver;
using CapManagement.Server.IRepository;
using CapManagement.Server.IService;
using CapManagement.Server.Repository;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.CarDtoModels;
using CapManagement.Shared.DtoModels.DriverDtoModels;
using CapManagement.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CapManagement.Server.Services
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _carRepository;

        public CarService(ICarRepository carRepository)
        {
            _carRepository = carRepository;
        }

        /// <summary>
        /// archive method which is soft delete is implmented instead of delete for auditing .
        /// </summary>
        public async Task<ApiResponse<bool>> ArchiveCarAsync(Guid carId, Guid companyId)
        {
           var response = new ApiResponse<bool>();

            try
            {
                // ✅ Step 1: Check if the car exists
                var car = await _carRepository.GetCarWithContractsAsync(carId, companyId);
                if (car == null)
                {
                    response.Success = false;
                    response.Data = false;
                    response.Errors.Add("Car not found or does not belong to this company.");
                    return response;
                }
                // ✅ Step 2: Prevent archiving if car has active contracts
                if (car.Contracts.Any(c => c.Status == ContractStatus.Active))
                {
                    response.Success = false;
                    response.Data = false;
                    response.Errors.Add("Car cannot be archived while it is assigned to an active contract. Please expire the contract first.");
                    return response;
                }


                var result = await _carRepository.ArchiveCarAsync(carId, companyId);
                if(!result)
                {
                    response.Success = false;
                    response.Data = false;
                    response.Errors.Add("car not found or already archived.");
                    return response;
                }


                response.Success = true;
                response.Data = true;
                response.Message = "car archived successfully.";
            }

            catch (Exception ex)
            {
                response.Success = false;
                response.Data = false;
                response.Errors.Add($"Error archiving driver: {ex.Message}");
            }

            return response;
        }


        /// <summary>
        /// Create car method is for creating a new car which has parameter of user
        /// </summary>
        public async Task<ApiResponse<CarDto>> CreateCarAsync(CreateCarDto carDto, string userId)
        {
            try
            {
                // Validate input
                var validationResponse = ValidateCreateCarDto(carDto, userId);
                if (!validationResponse.Success)
                {
                    return validationResponse;
                }

                // Check for existing number plate
                var existingNumberPlate = await _carRepository.GetCarByNumberPlateAsync(carDto.NumberPlate, carDto.CompanyId);
                if (existingNumberPlate != null)
                {
                    
                    return new ApiResponse<CarDto>
                    {
                        Success = false,
                        Errors = new List<string> { $"NumberPlate '{carDto.NumberPlate}' already exists for this company." }
                    };
                }

                // Map to entity
                var car = new Car
                {
                    CarId = Guid.NewGuid(),
                    Brand = carDto.Brand,
                    Model = carDto.Model,
                    NumberPlate = carDto.NumberPlate,
                    Year = carDto.Year,
                    CompanyId = carDto.CompanyId,
                    Status = carDto.Status,
                    Color = carDto.Color,
                    VehicleType = carDto.VehicleType,
                    FuelType = carDto.FuelType,
                    NumberOfDoors = TryParseNullableInt(carDto.NumberOfDoors),
                    NumberOfSeats = TryParseNullableInt(carDto.NumberOfSeats),
                    EngineCapacity = TryParseNullableInt(carDto.EngineCapacity),
                    EmptyWeight = TryParseNullableInt(carDto.EmptyWeight),
                    ApkExpirationDate = TryParseNullableDate(carDto.ApkExpirationDate),
                    CreatedAt = DateTime.UtcNow
                };
                // Save to database via repository
                await _carRepository.CreateCarAsync(car);
               

                // Map to DTO
                var carDtoResult = new CarDto
                {
                    CarId = car.CarId,
                    Brand = car.Brand,
                    Model = car.Model,
                    NumberPlate = car.NumberPlate,
                    CompanyId = car.CompanyId,
                    Status = car.Status,
                      Color = carDto.Color,
                    VehicleType = carDto.VehicleType,
                    FuelType = carDto.FuelType,
                    NumberOfDoors = carDto.NumberOfDoors,
                    NumberOfSeats = carDto.NumberOfSeats,
                    EngineCapacity = carDto.EngineCapacity,
                    EmptyWeight = carDto.EmptyWeight,
                    ApkExpirationDate = carDto.ApkExpirationDate,

                };

                return new ApiResponse<CarDto>
                {
                    Success = true,
                    Data = carDtoResult
                };
            }
            catch (DbUpdateException ex)
            {
               
                return new ApiResponse<CarDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Database error: {ex.InnerException?.Message ?? ex.Message}" }
                };
            }
            catch (Exception ex)
            {

                return new ApiResponse<CarDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Unexpected error: {ex.Message}" }
                };
            }
        }



        private int? TryParseNullableInt(string? value)
        {
            return int.TryParse(value, out var result) ? result : null;
        }

        private DateTime? TryParseNullableDate(string? value)
        {
            return DateTime.TryParse(value, out var date) ? date : null;
        }


        // submethod for validating only the cars which are has validated value
        private ApiResponse<CarDto> ValidateCreateCarDto(CreateCarDto carDto, string userId)
        {
            var response = new ApiResponse<CarDto>();

            if (carDto == null)
            {
                response.Errors.Add("Car data is required.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(carDto.Brand))
                {
                    response.Errors.Add("Car Brand is required.");
                }
                if (string.IsNullOrWhiteSpace(carDto.Model))
                {
                    response.Errors.Add("Car Model is required.");
                }
                if (string.IsNullOrWhiteSpace(carDto.NumberPlate))
                {
                    response.Errors.Add("Car NumberPlate is required.");
                }
                if (carDto.CompanyId == Guid.Empty)
                {
                    response.Errors.Add("Company ID is required.");
                }
                if (!Enum.IsDefined(typeof(CarStatus), carDto.Status))
                {
                    response.Errors.Add($"Invalid Car Status: {carDto.Status}. Valid values are: {string.Join(", ", Enum.GetNames(typeof(CarStatus)))}.");
                }
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                response.Errors.Add("User ID is required.");
            }

            if (response.Errors.Any())
            {
                response.Success = false;
            }
            else
            {
                response.Success = true;
            }

            return response;
        }



        /// <summary>
        /// Retrieves a paginated list of active cars for a specific company.
        /// Only cars that are currently active (not archived or disabled) are included.
        /// </summary>
        /// <param name="companyId">
        /// The unique identifier of the company whose active cars should be retrieved.
        /// </param>
        /// <param name="pageNumber">
        /// The page number to retrieve (starting from 1).
        /// </param>
        /// <param name="pageSize">
        /// The number of cars to include per page.
        /// </param>
        /// <returns>
        public async Task<ApiResponse<PagedResponse<CarDto>>> GetAllCarAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
        {
            var response = new ApiResponse<PagedResponse<CarDto>>();

            var pagedResult = await _carRepository.GetAllCarAsync(pageNumber, pageSize, companyId, orderBy, filter);

            var pagedResponse = new PagedResponse<CarDto>
            {
                Data = pagedResult.Items.Select(c => new CarDto
                {
                    CarId = c.CarId,
                    Brand = c.Brand,
                    Model = c.Model,
                    NumberPlate = c.NumberPlate,
                    Year = c.Year,
                    Status = c.Status,          // ✅ CarStatus enum
                    CompanyId = c.CompanyId,
                    // ✅ RDW fields
                    Color = c.Color,
                    // ✅ RDW fields (converted to strings)
                    
                    VehicleType = c.VehicleType ?? string.Empty,
                    FuelType = c.FuelType ?? string.Empty,
                    NumberOfDoors = c.NumberOfDoors?.ToString() ?? string.Empty,
                    NumberOfSeats = c.NumberOfSeats?.ToString() ?? string.Empty,
                    EngineCapacity = c.EngineCapacity?.ToString() ?? string.Empty,
                    EmptyWeight = c.EmptyWeight?.ToString() ?? string.Empty,
                    ApkExpirationDate = c.ApkExpirationDate?.ToString("dd MMM yyyy") ?? string.Empty,
                    //FirstRegistrationDate = c.FirstRegistrationDate?.ToString("dd MMM yyyy") ?? string.Empty
                }).ToList(),
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize,
                TotalPages = (int)Math.Ceiling(pagedResult.TotalCount / (double)pagedResult.PageSize),
                TotalItems = pagedResult.TotalCount
            };

            response.Success = true;
            response.Data = pagedResponse;
            return response;
        }



        /// <summary>
        /// Retrieves a paginated list of archived (inactive) cars for a specific company.
        /// Archived cars are vehicles that are no longer active but kept for historical or audit purposes.
        /// </summary>
        /// <param name="pageNumber">
        /// The page number to retrieve (starting from 1).
        /// </param>
        /// <param name="pageSize">
        /// The number of cars to include per page.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company whose archived cars should be retrieved.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a paginated list of archived cars,
        /// including pagination metadata such as total pages and total items.
        /// </returns>

        public async Task<ApiResponse<PagedResponse<CarDto>>> GetArchivedCarsAsync(int pageNumber, int pageSize, Guid companyId)
        {
            var response = new ApiResponse<PagedResponse<CarDto>>();
            var pagedResult = await _carRepository.GetArchivedCarAsync(pageNumber, pageSize, companyId);

            var pagedResponse = new PagedResponse<CarDto>
            {
                Data = pagedResult.Items.Select(c => new CarDto
                {
                    CarId = c.CarId,
                    Brand = c.Brand,
                    Model = c.Model,
                    NumberPlate = c.NumberPlate,
                    Year = c.Year,
                    Status = c.Status,          // ✅ CarStatus enum
                    CompanyId = c.CompanyId
                }).ToList(),
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize,
                TotalPages = (int)Math.Ceiling(pagedResult.TotalCount / (double)pagedResult.PageSize),
                TotalItems = pagedResult.TotalCount
            };

            response.Success = true;
            response.Data = pagedResponse;
            return response;
        }


        /// <summary>
        /// Retrieves a specific car by its ID for a given company.
        /// Ensures the car belongs to the provided company before returning it.
        /// </summary>
        /// <param name="carId">
        /// The unique identifier of the car to retrieve.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company that owns the car.
        /// </param>
        /// <returns>
        /// A <see cref="CarDto"/> if the car is found; otherwise, <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when either <paramref name="carId"/> or <paramref name="companyId"/> is empty.
        /// </exception>
        public async Task<CarDto> GetCarByIdAsync(Guid carId, Guid companyId)
        {
            if (carId == Guid.Empty)
                throw new ArgumentException("car ID cannot be empty.", nameof(carId));

            if (companyId == Guid.Empty)
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

            // Fetch driver from repository
            var car = await _carRepository.GetCarByIdAsync(carId, companyId);

            if (car == null)
                return null;

            // Map entity to DTO
            return new CarDto
            {
                CarId = car.CarId,
                Brand = car.Brand,
                Model = car.Model,
                NumberPlate = car.NumberPlate,
                Year = car.Year,
                Status = car.Status,          // ✅ CarStatus enum
                CompanyId = car.CompanyId
            };

            }




        /// <summary>
        /// Restores a previously archived car and makes it active again for a specific company.
        /// </summary>
        /// <param name="carId">
        /// The unique identifier of the car to be restored.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company that owns the car.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating whether the restore operation was successful.
        /// </returns>
        public async Task<ApiResponse<bool>> RestoreCarAsync(Guid carId, Guid companyId)
        {
            var response = new ApiResponse<bool>();

            try
            {
                var result = await _carRepository.RestoreCarAsync(carId, companyId);

                if (!result)
                {
                    response.Success = false;
                    response.Errors = new List<string> { "car not found or failed to restore." };
                    return response;
                }

                response.Success = true;
                response.Data = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Errors = new List<string> { "An error occurred while restoring the driver.", ex.Message };
                // Optional: log the exception here
            }

            return response;
        }



        /// <summary>
        /// Updates an existing car for a specific company after validating ownership,
        /// uniqueness, and business rules.
        /// </summary>
        /// <param name="carDto">
        /// The car data transfer object containing updated car information.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating whether the update operation was successful.
        /// </returns>
        public async Task<ApiResponse<bool>> UpdateCarAsync(CarDto carDto)
        {
            var response = new ApiResponse<bool>();

            try
            {
                // ✅ Validation
                if (carDto == null)
                {
                    response.Success = false;
                    response.Errors = new List<string> { "Car data is required." };
                    return response;
                }

                if (carDto.CompanyId == Guid.Empty)
                {
                    response.Success = false;
                    response.Errors = new List<string> { "Company ID is required." };
                    return response;
                }

                // ✅ Check if car exists
                var existingCar = await _carRepository.GetCarByIdAsync(carDto.CarId, carDto.CompanyId);
                if (existingCar == null)
                {
                    response.Success = false;
                    response.Errors = new List<string> { "Car not found, inactive, or does not belong to the specified company." };
                    return response;
                }

                // ✅ Check for duplicate NumberPlate
                var duplicate = await _carRepository.GetCarByNumberPlateAsync(carDto.NumberPlate, carDto.CompanyId);
                if (duplicate != null && duplicate.CarId != carDto.CarId)
                {
                    response.Success = false;
                    response.Errors = new List<string> { $"A car with NumberPlate '{carDto.NumberPlate}' already exists in this company." };
                    return response;
                }

                // ✅ Update properties
                existingCar.Brand = carDto.Brand;
                existingCar.Model = carDto.Model;
                existingCar.NumberPlate = carDto.NumberPlate;
                existingCar.Year = carDto.Year;
                existingCar.Status = carDto.Status;
                existingCar.UpdatedAt = DateTime.UtcNow;

                await _carRepository.UpdateCarAsync(existingCar);

                response.Success = true;
                response.Data = true;
                return response;
            }
            catch (DbUpdateException ex)
            {
                response.Success = false;
                response.Errors = new List<string> { $"Database error: {ex.InnerException?.Message ?? ex.Message}" };
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Errors = new List<string> { $"Unexpected error: {ex.Message}" };
                return response;
            }
        }
    }
}
