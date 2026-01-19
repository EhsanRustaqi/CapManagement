using CapManagement.Server.IRepository;
using CapManagement.Server.IService;
using CapManagement.Server.Repository;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.CompanyDtoModels;
using CapManagement.Shared.DtoModels.DriverDtoModels;
using CapManagement.Shared.Models;

namespace CapManagement.Server.Services
{
    public class DriverService : IDriverService
    {
        private readonly IDriverRepository _driverRepository;

        public DriverService(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository;
        }




        /// <summary>
        /// Archives (soft deletes) a driver so they are no longer active in the system.
        /// </summary>
        /// <param name="driverId">
        /// The unique identifier of the driver to archive.
        /// </param>
        /// <param name="companyId">
        /// The company to which the driver belongs.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating whether the archive operation was successful.
        /// </returns>

        public async Task<ApiResponse<bool>> ArchiveDriverAsync(Guid driverId, Guid companyId)
        {
            var response = new ApiResponse<bool>();

            try
            {
                var result = await _driverRepository.ArchiveDriverAsync(driverId, companyId);

                if (!result)
                {
                    response.Success = false;
                    response.Data = false;
                    response.Errors.Add("Driver not found or already archived.");
                    return response;
                }

                response.Success = true;
                response.Data = true;
                response.Message = "Driver archived successfully.";
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
        /// Creates a new driver within a company after validating business rules.
        /// </summary>
        /// <param name="driverDto">
        /// The DTO containing driver details submitted by the client.
        /// </param>
        /// <param name="userId">
        /// The ID of the user creating the driver (used for auditing/ownership).
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the created driver or validation errors.
        /// </returns>
        public async Task<ApiResponse<DriverDto>> CreateDriverAsync(CreateDriverDto driverDto, string userId)
        {
            var response = new ApiResponse<DriverDto>();

            // 1. Validate input DTO
            if (driverDto == null)
            {
                response.Success = false;
                response.Message = "Driver data is required.";
                return response;
            }

            if (string.IsNullOrWhiteSpace(driverDto.DriverName))
            {
                response.Success = false;
                response.Message = "Driver first name is required.";
                return response;
            }

            if (string.IsNullOrWhiteSpace(driverDto.LastName))
            {
                response.Success = false;
                response.Message = "Driver last name is required.";
                return response;
            }

            if (string.IsNullOrWhiteSpace(driverDto.LicenseNumber))
            {
                response.Success = false;
                response.Message = "License number is required.";
                return response;
            }

            if (driverDto.CompanyId == Guid.Empty)
            {
                response.Success = false;
                response.Message = "Company ID is required.";
                return response;
            }

            // 2. Check for duplicate license number
            var existingLicenseDriver = await _driverRepository.GetDriverByLiecnseNumberAsync(
                driverDto.LicenseNumber, driverDto.CompanyId);

            if (existingLicenseDriver != null)
            {
                response.Success = false;
                response.Message = "A driver with this license number already exists in the company.";
                return response;
            }

            // 3. Check for duplicate name
            var existingNameDriver = await _driverRepository.GetDriverByNameAsync(
                driverDto.DriverName, driverDto.LastName, driverDto.CompanyId);

            if (existingNameDriver != null)
            {
                response.Success = false;
                response.Message = "A driver with this name already exists in the company.";
                return response;
            }

            // 4. Map DTO to entity
            var driver = new Driver
            {
                DriverId = Guid.NewGuid(),
                DriverName = driverDto.DriverName,
                LastName = driverDto.LastName,
                DateOfBirth = driverDto.DateOfBirth,
                Phonenumber = driverDto.Phonenumber,
                CompanyId = driverDto.CompanyId,
                LicenseNumber = driverDto.LicenseNumber,
                UserId = Guid.Parse(userId),
                IsActive = true
            };

            // 5. Save via repository
            var createdDriver = await _driverRepository.CreateDriverAsync(driver);

            // 6. Map entity back to DTO
            response.Data = new DriverDto
            {
                DriverId = createdDriver.DriverId,
                DriverName = createdDriver.DriverName,
                LastName = createdDriver.LastName,
                DateOfBirth = createdDriver.DateOfBirth,
                Phonenumber = createdDriver.Phonenumber,
                CompanyId = createdDriver.CompanyId,
                LicenseNumber = createdDriver.LicenseNumber,
                IsActive = createdDriver.IsActive
            };

            response.Success = true;
            response.Message = "Driver created successfully.";

            return response;
        }



        /// <summary>
        /// Retrieves a paginated list of all active drivers for a specific company.
        /// </summary>
        /// <param name="pageNumber">
        /// The current page number (1-based).
        /// </param>
        /// <param name="pageSize">
        /// The number of drivers to return per page.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company whose drivers are requested.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a paged list of drivers.
        /// </returns>
        public async Task<ApiResponse<PagedResponse<DriverDto>>> GetAllDriverAsync(int pageNumber, int pageSize, Guid companyId)
        {
            var response = new ApiResponse<PagedResponse<DriverDto>>();

            var pagedResult = await _driverRepository.GetAllDriversAsync(pageNumber, pageSize, companyId);

            var pagedResponse = new PagedResponse<DriverDto>
            {
                Data = pagedResult.Items.Select(c => new DriverDto
                {
                    DriverId = c.DriverId,
                    DriverName = c.DriverName,
                    LastName = c.LastName,
                    LicenseNumber = c.LicenseNumber,
                    DateOfBirth = c.DateOfBirth,
                    Phonenumber = c.Phonenumber,
                    CompanyId = c.CompanyId,
                    IsActive = c.IsActive,
                    DeletedAt = c.DeletedAt
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
        /// Retrieves a paginated list of archived (inactive) drivers for a specific company.
        /// </summary>
        /// <param name="pageNumber">
        /// The current page number (1-based).
        /// </param>
        /// <param name="pageSize">
        /// The number of drivers to return per page.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company whose archived drivers are requested.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a paged list of archived drivers.
        /// </returns>
        public async Task<ApiResponse<PagedResponse<DriverDto>>> GetArchivedDriversAsync(int pageNumber, int pageSize, Guid companyId)
        {
            var response = new ApiResponse<PagedResponse<DriverDto>>();
            var pagedResult = await _driverRepository.GetArchivedDriversAsync(pageNumber, pageSize,companyId);

            var pagedResponse = new PagedResponse<DriverDto>
            {
                Data = pagedResult.Items.Select(c => new DriverDto
                {
                    DriverId = c.DriverId,
                    DriverName = c.DriverName,
                    LastName = c.LastName,
                    DateOfBirth = c.DateOfBirth,
                    Phonenumber = c.Phonenumber,
                    CompanyId = c.CompanyId,
                    LicenseNumber = c.LicenseNumber,
                    IsActive = c.IsActive
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
        /// Retrieves a specific driver by ID within a given company.
        /// </summary>
        /// <param name="driverId">
        /// The unique identifier of the driver.
        /// </param>
        /// <param name="companyId">
        /// The company to which the driver belongs.
        /// </param>
        /// <returns>
        /// A <see cref="DriverDto"/> if found; otherwise, null.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when required identifiers are empty.
        /// </exception>
        public async Task<DriverDto> GetDriverByIdAsync(Guid driverId, Guid companyId)
        {
            if (driverId == Guid.Empty)
                throw new ArgumentException("Driver ID cannot be empty.", nameof(driverId));

            if (companyId == Guid.Empty)
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

            // Fetch driver from repository
            var driver = await _driverRepository.GetDriverByIdAsync(driverId, companyId);

            if (driver == null)
                return null;

            // Map entity to DTO
            return new DriverDto
            {
                DriverId = driver.DriverId,
                DriverName = driver.DriverName,
                LastName = driver.LastName,
                DateOfBirth = driver.DateOfBirth,
                Phonenumber = driver.Phonenumber,
                CompanyId = driver.CompanyId,
                LicenseNumber = driver.LicenseNumber,
                IsActive = driver.IsActive,
                DeletedAt = driver.DeletedAt
            };
        }



        /// <summary>
        /// Restores a previously archived driver and marks them as active again.
        /// </summary>
        /// <param name="driverId">
        /// The unique identifier of the driver to restore.
        /// </param>
        /// <param name="companyId">
        /// The company to which the driver belongs.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating whether the restore operation was successful.
        /// </returns>
        public async Task<ApiResponse<bool>> RestoreDriverAsync(Guid driverId, Guid companyId)
        {
            var response = new ApiResponse<bool>();

            try
            {
                var result = await _driverRepository.RestoreDriverAsync(driverId, companyId);

                if (!result)
                {
                    response.Success = false;
                    response.Errors = new List<string> { "Driver not found or failed to restore." };
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
        /// Updates an existing driver's information.
        /// </summary>
        /// <param name="driverDto">
        /// The DTO containing updated driver details.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating whether the update operation was successful.
        /// </returns>
        public async Task<ApiResponse<bool>> UpdateDriverAsync(DriverDto driverDto)
        {
            var response = new ApiResponse<bool>();

            if (driverDto == null)
            {
                response.Success = false;
                response.Message = "Driver data is required.";
                return response;
            }

            if (driverDto.DriverId == Guid.Empty)
            {
                response.Success = false;
                response.Message = "Driver ID cannot be empty.";
                return response;
            }

            if (driverDto.CompanyId == Guid.Empty)
            {
                response.Success = false;
                response.Message = "Company ID cannot be empty.";
                return response;
            }

            try
            {
                // Fetch the existing driver from DB
                var existingDriver = await _driverRepository.GetDriverByIdAsync(driverDto.DriverId, driverDto.CompanyId);
                if (existingDriver == null)
                {
                    response.Success = false;
                    response.Message = "Driver not found.";
                    return response;
                }

                // Update properties
                existingDriver.DriverName = driverDto.DriverName;
                existingDriver.LastName = driverDto.LastName;
                existingDriver.DateOfBirth = driverDto.DateOfBirth;
                existingDriver.Phonenumber = driverDto.Phonenumber;
                existingDriver.LicenseNumber = driverDto.LicenseNumber;
                existingDriver.IsActive = driverDto.IsActive;
                existingDriver.DeletedAt = driverDto.DeletedAt;
                existingDriver.UpdatedAt = DateTime.UtcNow;

                var updated = await _driverRepository.UpdateDriverAsync(existingDriver);

                response.Success = updated;
                response.Data = updated;
                response.Message = updated ? "Driver updated successfully." : "Failed to update driver.";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating driver: {ex.Message}";
                return response;
            }
        }

    }
}
