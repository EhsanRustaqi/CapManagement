using CapManagement.Server.DbContexts;
using CapManagement.Server.IRepository;
using CapManagement.Server.IService;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.CarDtoModels;
using CapManagement.Shared.DtoModels.ContractDto;
using CapManagement.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Data;
namespace CapManagement.Server.Services
{
    public class ContractService : IContractService
    {

        private readonly ICarRepository _carRepository;
        private readonly IContractRepository _contractRepository;
        private readonly FleetDbContext _context;
        public ContractService(IContractRepository contractRepository
            , ICarRepository carRepository, FleetDbContext context)
        {
            _contractRepository = contractRepository;
            _carRepository = carRepository;
            _context = context;
        }


        /// <summary>
        /// Archives a contract for a specific company.
        /// Archived contracts are deactivated but kept for historical records.
        /// </summary>
        /// <param name="contractId">
        /// The unique identifier of the contract to archive.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company that owns the contract.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating whether the archive operation was successful.
        /// </returns>

        public async Task<ApiResponse<bool>> ArchiveContractAsync(Guid contractId, Guid companyId)
        {
            var response = new ApiResponse<bool>();

            try
            {
                var result = await _contractRepository.ArchiveContractAsync(contractId, companyId);
                if (!result)
                {
                    response.Success = false;
                    response.Data = false;
                    response.Errors.Add("contract not found or already archived.");
                    return response;   // ← here response.Success stays false → triggers 404
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Data = false;
                response.Errors.Add($"Error archiving driver: {ex.Message}");
            }

            return response;  // ← ⚠️ problem: Success is never set to true when result == true
        }



        /// <summary>
        /// Creates a new contract for a car in a safe and atomic way, preventing duplicate
        /// active contracts for the same vehicle and validating all business rules.
        /// </summary>
        /// <param name="contractDto">
        /// The contract creation data provided by the client.
        /// </param>
        /// <param name="userId">
        /// The identifier of the user creating the contract.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the created contract if successful,
        /// or validation / business errors otherwise.
        /// </returns>
        public async Task<ApiResponse<ContractDto>> CreateContractAsync(CreateContractDto contractDto, string userId)
        {

            // temporary hardcoded company id until auth is ready
            var hardcodedCompanyId = Guid.Parse("9D176E43-E0FF-4755-B130-625189F3991B");
            contractDto.CompanyId = hardcodedCompanyId; // override whatever comes in request

            // Step 1: Validate inputs
            var validationErrors = ValidateContractInput(contractDto, userId);
            if (validationErrors.Any())
            {
                return new ApiResponse<ContractDto>
                {
                    Success = false,
                    Errors = validationErrors
                };
            }

            // Step 2: Check if car exists and is active (outside transaction—read-only, minimizes lock time)
            var car = await _carRepository.GetCarBasicInfoAsync(contractDto.CarId, contractDto.CompanyId);
            if (car == null)
            {
                return new ApiResponse<ContractDto>
                {
                    Success = false,
                    Errors = new() { $"Car with ID {contractDto.CarId} does not exist or is inactive." }
                };
            }

            // Step 3: Atomic block (transaction with Serializable isolation to prevent double creates/race conditions)
            IDbContextTransaction? transaction = null;
            try
            {
                transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                // Re-check inside transaction for atomicity (prevents concurrent requests from both seeing "no existing")
                var existingContract = await _contractRepository.GetActiveContractByCarAsync(contractDto.CarId, contractDto.CompanyId);
                if (existingContract != null)
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<ContractDto>
                    {
                        Success = false,
                        Errors = new() { $"Car with number plate {car.NumberPlate} is already in an active contract." }
                    };
                }

                // Step 4: Validate PDF (quick, so inside for full atomicity)
                var pdfErrors = ValidatePdf(contractDto.PdfContent);
                if (pdfErrors.Any())
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<ContractDto>
                    {
                        Success = false,
                        Errors = pdfErrors
                    };
                }

                // Step 5: Map DTO to entity
                var contract = MapToContractEntity(contractDto);

                // Step 6: Persist in repository (uses shared _context; SaveChangesAsync inside repo)
                var createdContract = await _contractRepository.CreateContractAsync(contract);

                // Step 7: Commit transaction (releases locks; second concurrent request now sees the new contract)
                await transaction.CommitAsync();

                // Step 8: Map entity to DTO
                var contractDtoResult = MapToContractDto(createdContract);

                return new ApiResponse<ContractDto>
                {
                    Success = true,
                    Data = contractDtoResult,
                    Message = "Contract created successfully"
                };
            }
            catch (Exception ex)
            {
                // Explicit rollback on any error (including deadlocks from Serializable)
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                // Log ex for debugging (inject ILogger<YourService> and use _logger.LogError(ex, "CreateContract failed");)
                return new ApiResponse<ContractDto>
                {
                    Success = false,
                    Errors = new() { $"Unexpected error: {ex.Message}" }
                };
            }

        }



        /// <summary>
        /// Retrieves a paginated list of all contracts for a specific company,
        /// with optional sorting and filtering.
        /// </summary>
        /// <param name="pageNumber">
        /// The page number to retrieve (starting from 1).
        /// </param>
        /// <param name="pageSize">
        /// The number of contracts to include per page.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company whose contracts should be retrieved.
        /// </param>
        /// <param name="orderBy">
        /// Optional sorting expression (e.g. "StartDate desc", "PaymentAmount asc").
        /// </param>
        /// <param name="filter">
        /// Optional filter value to search contracts by driver, car, or other criteria.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a paginated list of contracts,
        /// including pagination metadata such as total pages and total items.
        /// </returns>
        public async Task<ApiResponse<PagedResponse<ContractDto>>> GetAllContractAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
        {
            var response = new ApiResponse<PagedResponse<ContractDto>>();

            var pagedResult = await _contractRepository.GetAllContractAsync(pageNumber, pageSize, companyId, orderBy, filter);

            var pagedResponse = new PagedResponse<ContractDto>
            {
                Data = pagedResult.Items.Select(c => new ContractDto
                {
                    ContractId = c.ContractId,
                    CompanyId = c.CompanyId,
                    DriverId = c.DriverId,
                    DriverName = c.Driver != null ? c.Driver.DriverName : string.Empty,
                    CarId = c.CarId,
                    //CarName = c.Car != null ? c.Car.Brand : string.Empty,
                    //                CarName = c.Car != null
                    //? (!string.IsNullOrEmpty(c.Car.Brand)
                    //    ? c.Car.Brand
                    //    : (!string.IsNullOrEmpty(c.Car.NumberPlate)
                    //        ? c.Car.NumberPlate
                    //        : string.Empty))
                    //: string.Empty,
                    CarName = c.Car != null
                  ? $"{c.Car.NumberPlate ?? ""} — {c.Car.Brand ?? ""} {c.Car.Model ?? ""}".Trim()
                  : string.Empty,


                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status,               // ContractStatus
                    PaymentAmount = c.PaymentAmount,
                    Description = c.Description,
                    Conditions = c.Conditions
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
        /// Retrieves a paginated list of archived (inactive) contracts for a specific company,
        /// with optional sorting and filtering.
        /// Archived contracts are retained for historical, reporting, and audit purposes.
        /// </summary>
        /// <param name="pageNumber">
        /// The page number to retrieve (starting from 1).
        /// </param>
        /// <param name="pageSize">
        /// The number of contracts to include per page.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company whose archived contracts should be retrieved.
        /// </param>
        /// <param name="orderBy">
        /// Optional sorting expression (e.g. "EndDate desc", "PaymentAmount asc").
        /// </param>
        /// <param name="filter">
        /// Optional filter value to search archived contracts by driver, car, or other criteria.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a paginated list of archived contracts,
        /// including pagination metadata such as total pages and total items.
        /// </returns>
        public async Task<ApiResponse<PagedResponse<ContractDto>>> GetArchivedContractsAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
        {
            var response = new ApiResponse<PagedResponse<ContractDto>>();

            var pagedResult = await _contractRepository.GetArchivedContractAsync(pageNumber, pageSize, companyId, orderBy, filter);

            var pagedResponse = new PagedResponse<ContractDto>
            {
                Data = pagedResult.Items.Select(c => new ContractDto
                {
                    ContractId = c.ContractId,
                    CompanyId = c.CompanyId,
                    DriverId = c.DriverId,
                    DriverName = c.Driver != null ? c.Driver.DriverName : string.Empty,
                    CarId = c.CarId,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status,               // ContractStatus
                    PaymentAmount = c.PaymentAmount,
                    Description = c.Description,
                    Conditions = c.Conditions
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
        /// Retrieves a specific contract by its unique identifier within a given company.
        /// Ensures the contract belongs to the specified company for data isolation and security.
        /// </summary>
        /// <param name="contractId">
        /// The unique identifier of the contract to retrieve.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company that owns the contract.
        /// </param>
        /// <returns>
        /// A <see cref="ContractDto"/> if found; otherwise, <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="contractId"/> or <paramref name="companyId"/> is empty.
        /// </exception>
        public async Task<ContractDto> GetContractByIdAsync(Guid contractId, Guid companyId)
        {
            if(contractId == Guid.Empty)
            {
                throw new ArgumentException("car ID cannot be empty.", nameof(contractId));

            }



            if (companyId == Guid.Empty)
            {
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));
            }

            var contract = await _contractRepository.GetContractByIdAsync(contractId, companyId);
            if (contract == null)
            
                return null;

            return new ContractDto

            {
                ContractId = contract.ContractId,
                DriverId = contract.DriverId,
                DriverName = contract.Driver != null ? contract.Driver.DriverName : string.Empty,
                CarId = contract.CarId,
                CarName = contract.Car != null
                  ? $"{contract.Car.NumberPlate ?? ""} — {contract.Car.Brand ?? ""} {contract.Car.Model ?? ""}".Trim()
                  : string.Empty,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = contract.Status,               // ContractStatus
                PaymentAmount = contract.PaymentAmount,
                Description = contract.Description,
                Conditions = contract.Conditions




            };

            

        }


        /// <summary>
        /// Retrieves the PDF document associated with a specific contract.
        /// Ensures the contract belongs to the specified company for security and isolation.
        /// </summary>
        /// <param name="contractId">
        /// The unique identifier of the contract whose PDF is requested.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company that owns the contract.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the PDF file as a byte array if found;
        /// otherwise, an error response.
        /// </returns>
        public async Task<ApiResponse<byte[]>> GetContractPdfAsync(Guid contractId, Guid companyId)
        {

            var response = new ApiResponse<byte[]>();

            if (contractId == Guid.Empty || companyId == Guid.Empty)
            {
                response.Success = false;
                response.Errors.Add("Invalid contract or company ID.");
                return response;
            }

            var pdfContent = await _contractRepository.GetContractPdfAsync(contractId, companyId);

            if (pdfContent == null)
            {
                response.Success = false;
                response.Errors.Add("PDF not found.");
                return response;
            }

            response.Success = true;
            response.Data = pdfContent;
            response.Message = "PDF retrieved successfully.";
            return response;
        }


        /// <summary>
        /// Restores an archived contract, making it active again for the specified company.
        /// </summary>
        /// <param name="contractId">
        /// The unique identifier of the contract to restore.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company that owns the contract.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating whether the restore operation was successful.
        /// </returns>

        public async Task<ApiResponse<bool>> RestoreContractAsync(Guid contractId, Guid companyId)
        {
            var response = new ApiResponse<bool>();

            try
            {
                var result = await _contractRepository.RestoreContractAsync(contractId, companyId);

                if (!result)
                {
                    response.Success = false;
                    response.Errors = new List<string> { "contract not found or failed to restore." };
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
        /// Updates an existing contract with new details.
        /// </summary>
        /// <param name="updateContractDto">
        /// The DTO containing updated contract information.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating whether the update operation was successful.
        /// </returns>
        public async Task<ApiResponse<bool>> UpdateContractAsync(UpdateContractDto updateContractDto)


        {
            var response = new ApiResponse<bool>();

            try
            {
                // ✅ Validation
                if (updateContractDto == null)
                {
                    response.Success = false;
                    response.Errors = new List<string> { "Car data is required." };
                    return response;
                }

                if (updateContractDto.CompanyId == Guid.Empty)
                {
                    response.Success = false;
                    response.Errors = new List<string> { "updateContractDto ID is required." };
                    return response;
                }

                // ✅ Check if car exists
                var existingContract = await _contractRepository.GetContractByIdAsync(updateContractDto.ContractId, updateContractDto.CompanyId);
                if (existingContract == null)
                {
                    response.Success = false;
                    response.Errors = new List<string> { "Car not found, inactive, or does not belong to the specified company." };
                    return response;
                };



                // ✅ Update properties
                // ✅ Update contract properties
                existingContract.DriverId = updateContractDto.DriverId;
                existingContract.CarId = updateContractDto.CarId;
                existingContract.StartDate = updateContractDto.StartDate;
                existingContract.EndDate = updateContractDto.EndDate;
                existingContract.Status = updateContractDto.Status;
                existingContract.PaymentAmount = updateContractDto.PaymentAmount;
                existingContract.Description = updateContractDto.Description;
                existingContract.Conditions = updateContractDto.Conditions;
                existingContract.UpdatedAt = DateTime.UtcNow;

                // If PDF content is passed during update (optional)
                if (updateContractDto.PdfContent != null && updateContractDto.PdfContent.Length > 0)
                {
                    existingContract.PdfContent = updateContractDto.PdfContent;
                }

                await _contractRepository.UpdateContractAsync(existingContract);

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







        // this method is for validating contract input 


        private List<string> ValidateContractInput(CreateContractDto dto, string userId)

        {
            var errors = new List<string>();

            if (dto == null) errors.Add("Contract data is required.");
            if (string.IsNullOrWhiteSpace(userId)) errors.Add("User ID is required.");
            if (dto?.CompanyId == Guid.Empty) errors.Add("Company ID cannot be empty.");
            if (dto?.DriverId == Guid.Empty) errors.Add("Driver ID cannot be empty.");
            if (dto?.CarId == Guid.Empty) errors.Add("Car ID cannot be empty.");
            if (dto?.StartDate < DateTime.UtcNow.Date) errors.Add("Start date cannot be in the past.");
            if (dto?.EndDate.HasValue == true && dto.EndDate <= dto.StartDate) errors.Add("End date must be after start date.");
            if (dto?.PaymentAmount < 0) errors.Add("Payment amount cannot be negative.");
            if (dto?.Status != ContractStatus.Active) errors.Add("New contracts must have status Pending.");

            return errors;
        }






        /// <summary>
        /// Validates the uploaded PDF file for size and format.
        /// </summary>
        /// <param name="pdfContent">
        /// The PDF file content as a byte array. Can be null if no PDF is provided.
        /// </param>
        /// <returns>
        /// A list of validation error messages. 
        /// If the list is empty, the PDF is considered valid.
        /// </returns>

        private List<string> ValidatePdf(byte[]? pdfContent)
        {
            var errors = new List<string>();
            if (pdfContent == null) return errors;

            if (pdfContent.Length > 10 * 1024 * 1024)
                errors.Add("PDF file size exceeds 10MB limit.");

            if (!(pdfContent.Length > 4 && pdfContent[0] == 0x25 && pdfContent[1] == 0x50 && pdfContent[2] == 0x44 && pdfContent[3] == 0x46))
                errors.Add("Invalid PDF format.");

            return errors;
        }



        /// <summary>
        /// Maps a Contract entity to a ContractDto for API response.
        /// Hides raw PDF binary data and instead exposes metadata and access URL.
        /// </summary>
        /// <param name="contract">
        /// The contract entity retrieved from the database.
        /// </param>
        /// <returns>
        /// A <see cref="ContractDto"/> ready to be sent to the client.
        /// </returns>
        private ContractDto MapToContractDto(Contract contract)
        {
            return new ContractDto
            {
                ContractId = contract.ContractId,
                CompanyId = contract.CompanyId,
                DriverId = contract.DriverId,
                DriverName = contract.Driver?.DriverName ?? string.Empty,
                CarId = contract.CarId,
               
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = contract.Status,
                PaymentAmount = contract.PaymentAmount,
                Description = contract.Description,
                Conditions = contract.Conditions,
                // ✅ Instead of PdfContent, expose these:
                HasPdf = contract.PdfContent != null,
                PdfUrl = contract.PdfContent != null
            ? $"/api/contracts/{contract.ContractId}/pdf"
            : null
            };
        }

        /// <summary>
        /// Maps a CreateContractDto to a Contract entity for persistence.
        /// This method prepares a new contract record for database insertion.
        /// </summary>
        /// <param name="dto">
        /// The DTO containing contract data from the client request.
        /// </param>
        /// <returns>
        /// A fully initialized <see cref="Contract"/> entity.
        /// </returns>
        private Contract MapToContractEntity(CreateContractDto dto)
        {
            return new Contract
            {
                CompanyId = dto.CompanyId,
                DriverId = dto.DriverId,
              
                CarId = dto.CarId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = ContractStatus.Active,
                PaymentAmount = dto.PaymentAmount,
                Description = dto.Description ?? string.Empty,
                Conditions = dto.Conditions ?? string.Empty,
                PdfContent = dto.PdfContent,
                IsActive = true,
                IsDeleted = false // Soft delete default
            };
        }




        /// <summary>
        /// Marks an active contract as expired.
        /// </summary>
        /// <param name="contractId">
        /// The unique identifier of the contract to expire.
        /// </param>
        /// <param name="companyId">
        /// The company to which the contract belongs.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating whether the operation was successful.
        /// </returns>

        public async Task<ApiResponse<bool>> ExpireContractAsync(Guid contractId, Guid companyId)
        {
           var contract = await _contractRepository.
                GetContractByIdAsync(contractId, companyId);
            if (contract == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { "Contract not found." }
                };
            }

            contract.Status = ContractStatus.Expired;
            contract.UpdatedAt = DateTime.UtcNow;

            await _contractRepository.UpdateContractAsync(contract);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Contract expired successfully."
            };
        }

        public async Task<ApiResponse<PagedResponse<ContractDto>>> GetAllInActiveContractAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
        {
            var response = new ApiResponse<PagedResponse<ContractDto>>();

            var pagedResult = await _contractRepository.GetAllInActiveContractAsync(pageNumber, pageSize, companyId, orderBy, filter);

            var pagedResponse = new PagedResponse<ContractDto>
            {
                Data = pagedResult.Items.Select(c => new ContractDto
                {
                    ContractId = c.ContractId,
                    CompanyId = c.CompanyId,
                    DriverId = c.DriverId,
                    DriverName = c.Driver != null ? c.Driver.DriverName : string.Empty,
                    CarId = c.CarId,
                    //CarName = c.Car != null ? c.Car.Brand : string.Empty,
                    //                CarName = c.Car != null
                    //? (!string.IsNullOrEmpty(c.Car.Brand)
                    //    ? c.Car.Brand
                    //    : (!string.IsNullOrEmpty(c.Car.NumberPlate)
                    //        ? c.Car.NumberPlate
                    //        : string.Empty))
                    //: string.Empty,
                    CarName = c.Car != null
                  ? $"{c.Car.NumberPlate ?? ""} — {c.Car.Brand ?? ""} {c.Car.Model ?? ""}".Trim()
                  : string.Empty,


                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status,               // ContractStatus
                    PaymentAmount = c.PaymentAmount,
                    Description = c.Description,
                    Conditions = c.Conditions
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
    }

}
