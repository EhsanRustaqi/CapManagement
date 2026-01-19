using CapManagement.Client.Pages.Driver.Driver;
using CapManagement.Server.IRepository;
using CapManagement.Server.IService;
using CapManagement.Server.Repository;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.ContractDto;
using CapManagement.Shared.DtoModels.DriverDtoModels;
using CapManagement.Shared.DtoModels.EarningDtoModels;
using CapManagement.Shared.DtoModels.SettlementDtoModels;
using CapManagement.Shared.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CapManagement.Server.Services
{
    public class SettlmentService : ISettlmentService
    {

        private readonly ISettlementRepository _settlementRepository;
        private readonly IEarningRepository _earningRepository;
        private readonly ILogger<SettlmentService> _logger;

        public SettlmentService(
            ISettlementRepository settlementRepository,
            IEarningRepository earningRepository,
            ILogger<SettlmentService> logger)
        {
            _settlementRepository = settlementRepository;
            _earningRepository = earningRepository;
            _logger = logger;
        }


        /// <summary>
        /// Creates a new settlement and links the specified earnings to it.
        /// </summary>
        /// <param name="dto">The data transfer object containing settlement and earnings information.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the created <see cref="SettlementDto"/> if successful,
        /// or error details if the operation fails.
        /// </returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Validates the input DTO.
        /// 2. Maps the DTO to a settlement entity.
        /// 3. Validates and links earnings to the settlement.
        /// 4. Persists the settlement and related earnings atomically.
        /// 5. Recalculates financial totals after persistence.
        /// </remarks>
        /// <exception cref="Exception">
        /// Catches and logs unexpected errors during settlement creation.
        /// </exception>
        public async Task<ApiResponse<SettlementDto>> CreateSettlementAsync(CreateSettlementDto dto)
        {
            _logger.LogInformation("Starting settlement creation for contract {ContractId}", dto.ContractId);

            // 1. Validate input DTO
            var validationResult = ValidateCreateSettlmentDto(dto);
            if (!validationResult.Success)
            {
                _logger.LogWarning("Settlement validation failed: {Message}", validationResult.Message);
                return new ApiResponse<SettlementDto> { Success = false, Message = validationResult.Message };
            }

            try
            {
                // 2. Map DTO to entity
                var settlement = MapToSettlementEntity(dto);

                // 3. Link and validate earnings (fetches entities, sets SettlementId)
                var linkResult = await LinkAndValidateEarnings(dto.Earnings, settlement);
                if (!linkResult.Success)
                {
                    _logger.LogWarning("Earnings linking failed: {Message}", linkResult.Message);
                    return new ApiResponse<SettlementDto> { Success = false, Message = linkResult.Message };
                }
                var linkedEarnings = linkResult.Data;  // List<Earning>

                // 4. Save via repository (atomic insert + batch update)
                var savedSettlement = await _settlementRepository.CreateSettlementWithEarningsAsync(settlement, linkedEarnings);
                if (savedSettlement == null)
                {
                    _logger.LogError("Repository save failed for settlement {SettlementId}", settlement.SettlementId);
                    return new ApiResponse<SettlementDto> { Success = false, Message = "Failed to save settlement." };
                }

                // 5. Recalculate totals (your model method—loads navigation if needed)
                await _settlementRepository.LoadNavigationForRecalcAsync(savedSettlement);
                savedSettlement.RecalculateTotals();
                await _settlementRepository.UpdateSettlementAsync(savedSettlement);  // Persist updated calcs

                _logger.LogInformation("Settlement {SettlementId} created successfully with {EarningsCount} earnings", savedSettlement.SettlementId, linkedEarnings.Count);

                // 6. Map to output DTO
                var resultDto = MapToSettlementDto(savedSettlement);

                return new ApiResponse<SettlementDto> { Success = true, Data = resultDto, Message = "Settlement created successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating settlement for contract {ContractId}", dto.ContractId);
                return new ApiResponse<SettlementDto> { Success = false, Message = "Internal server error. Please try again." };
            }
        }

        /// <summary>
        /// Creates a new settlement and links the specified earnings to it.
        /// </summary>
        /// <param name="dto">The data transfer object containing settlement and earnings information.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the created <see cref="SettlementDto"/> if successful,
        /// or error details if the operation fails.
        /// </returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Validates the input DTO.
        /// 2. Maps the DTO to a settlement entity.
        /// 3. Validates and links earnings to the settlement.
        /// 4. Persists the settlement and related earnings atomically.
        /// 5. Recalculates financial totals after persistence.
        /// </remarks>
        /// <exception cref="Exception">
        /// Catches and logs unexpected errors during settlement creation.
        /// </exception>
        private ValidationResult ValidateCreateSettlmentDto(CreateSettlementDto dto)



        {
            if (dto == null)
            {
                return new ValidationResult { Success = false, Message = "Invalid request data." };
            }

            if (dto.ExtraCosts < 0)
            {
                return new ValidationResult { Success = false, Message = "Extra costs must be non-negative." };
            }

            if (!dto.Earnings.Any())
            {
                return new ValidationResult { Success = false, Message = "At least one earning is required." };
            }

            if (dto.PeriodEnd <= dto.PeriodStart)
            {
                return new ValidationResult { Success = false, Message = "Period end must be after start." };
            }

            // Sanitize description
            dto.Description = dto.Description?.Trim() ?? string.Empty;

            return new ValidationResult { Success = true };
        }

        /// <summary>
        /// Maps a <see cref="CreateSettlementDto"/> to a new <see cref="Settlement"/> entity.
        /// </summary>
        /// <param name="dto">The settlement creation DTO.</param>
        /// <returns>A new <see cref="Settlement"/> entity populated from the DTO.</returns>
        /// <remarks>
        /// This method creates a new settlement entity with default values such as
        /// a generated ID and initial status set to Pending.
        /// </remarks>
        private Settlement MapToSettlementEntity(CreateSettlementDto dto)


        {
            return new Settlement
            {
                SettlementId = Guid.NewGuid(),  // Generate if not auto
                CompanyId = dto.CompanyId,
                ContractId = dto.ContractId,
                PeriodStart = dto.PeriodStart,
                PeriodEnd = dto.PeriodEnd,
                ExtraCosts = dto.ExtraCosts,
                Description = dto.Description,
                Status = SettlementStatus.Pending  // Default

            };
        }



        // Sub-Function 3: Link/Validate Earnings (UPDATED: Uses your repo signature with companyId)
        private async Task<LinkResult> LinkAndValidateEarnings
            (List<CreateEarningDto> earningDtos, Settlement settlement)
        {
            if(earningDtos == null || !earningDtos.Any())
    {
                return new LinkResult { Success = false, Message = "No earnings provided for linking." };
            }

            var linkedEarnings = new List<Earning>();
            foreach (var earningDto in earningDtos)
            {
                if (earningDto.EarningId == Guid.Empty)
                {
                    // NEW: Create earning if ID Empty
                    var newEarning = await CreateNewEarningAsync(earningDto, settlement);
                    if (newEarning == null)
                    {
                        return new LinkResult { Success = false, Message = $"Failed to create new earning for platform {earningDto.Platform}." };
                    }
                    linkedEarnings.Add(newEarning);
                }
                else
                {
                    // EXISTING: Fetch and validate
                    var earning = await _earningRepository.GetEarningByIdAsync(earningDto.EarningId, settlement.CompanyId);
                    if (earning == null)
                    {
                        return new LinkResult { Success = false, Message = $"Earning {earningDto.EarningId} not found for company {settlement.CompanyId}." };
                    }

                    if (earning.ContractId != settlement.ContractId)
                    {
                        return new LinkResult { Success = false, Message = $"Earning {earningDto.EarningId} does not belong to contract {settlement.ContractId}." };
                    }

                    if (earning.SettlementId.HasValue)
                    {
                        return new LinkResult { Success = false, Message = $"Earning {earningDto.EarningId} is already linked to a settlement." };
                    }

                    // Check period fit
                    if (earning.WeekStart < settlement.PeriodStart || earning.WeekEnd > settlement.PeriodEnd)
                    {
                        return new LinkResult { Success = false, Message = $"Earning {earningDto.EarningId} week does not fit the settlement period." };
                    }

                    // Link
                    earning.SettlementId = settlement.SettlementId;
                    linkedEarnings.Add(earning);
                }
            }

            return new LinkResult { Success = true, Data = linkedEarnings };
        }





        // NEW Sub-Function: Create single new earning (calls repo)
        private async Task<Earning?> CreateNewEarningAsync(CreateEarningDto dto, Settlement settlement)
        {
            if (dto.WeekStart >= dto.WeekEnd) return null;

            dto.BtwAmount = dto.GrossIncome * (dto.BtwPercentage / 100m);
            dto.NetIncome = dto.GrossIncome - dto.BtwAmount;

            return new Earning
            {
                EarningId = Guid.Empty,                   // important: mark as new
                CompanyId = settlement.CompanyId,
                Platform = dto.Platform,
                ContractId = settlement.ContractId,
                SettlementId = settlement.SettlementId,   // temporary link (no DB write)
                WeekStart = dto.WeekStart,
                WeekEnd = dto.WeekEnd,
                GrossIncome = dto.GrossIncome,
                BtwPercentage = dto.BtwPercentage,
                IsActive = true
            };
        }

        // Sub-Function 4: Map Entity to Output DTO - Unchanged
        private SettlementDto MapToSettlementDto(Settlement entity)
        {
            return new SettlementDto
            {
                SettlementId = entity.SettlementId,
                CompanyId = entity.CompanyId,
                ContractId = entity.ContractId,
                PeriodStart = entity.PeriodStart,
                PeriodEnd = entity.PeriodEnd,
                GrossAmount = entity.GrossAmount,
                RentDeduction = entity.RentDeduction,
                NetPayout = entity.NetPayout,
                ExtraCosts = entity.ExtraCosts,
                Description = entity.Description,
                Status = entity.Status,
                ConfirmedAt = entity.ConfirmedAt,
                ConfirmedByDriver = entity.ConfirmedByDriver,
                Earnings = entity.Earnings.Select(e => new EarningDto
                {
                    EarningId = e.EarningId,
                    GrossIncome = e.GrossIncome,  // Map relevant props
                    // ... add other EarningDto fields as needed
                }).ToList()
            };
        }




        /// <summary>
        /// Retrieves a paginated list of settlements for a given company with optional ordering and filtering.
        /// </summary>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="orderBy">Optional ordering field.</param>
        /// <param name="filter">Optional filter expression.</param>
        /// <returns>A paged list of settlements.</returns>
        public async Task<ApiResponse<PagedResponse<SettlementDto>>> GetAllSettlementAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
        {
           var response = new ApiResponse<PagedResponse<SettlementDto>>();
            var pagedResult = await _settlementRepository.GetAllSettlmentsAsync(pageNumber, pageSize, companyId, orderBy, filter);
            var pagedResponse = new PagedResponse<SettlementDto>

            {
                Data = pagedResult.Items.Select(e => new SettlementDto
                {
                    SettlementId = e.SettlementId,
                    CompanyId = e.CompanyId,
                    //ContractId = e.ContractId,

                    Contracts = e.Contract != null ?
    new List<ContractDto>
    {
        new ContractDto
        {
            ContractId = e.Contract.ContractId,
            DriverName = e.Contract.Driver?.DriverName
        }
    }
    : new List<ContractDto>(),
                    PeriodStart = e.PeriodStart,
                    PeriodEnd = e.PeriodEnd,

                    GrossAmount = e.GrossAmount,
                    RentDeduction = e.RentDeduction,
                    NetPayout = e.NetPayout,
                    ExtraCosts = e.ExtraCosts,

                    Description = e.Description,
                    Status = e.Status,

                    ConfirmedAt = e.ConfirmedAt,
                    ConfirmedByDriver = e.ConfirmedByDriver,
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




     // below is the method for getting settlment by id 
        public async Task<SettlementDto> GetSettlementByIdAsync(Guid settlmentId, Guid companyId)
        {
            if (settlmentId == Guid.Empty)
                throw new ArgumentException("Driver ID cannot be empty.", nameof(settlmentId));

            if (companyId == Guid.Empty)
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

            var settlement = await _settlementRepository.GetSettlementByIdAsync(settlmentId, companyId);

            if (settlement == null)
                return null;

            return new SettlementDto
            {
                SettlementId = settlement.SettlementId,
                CompanyId = settlement.CompanyId,
                ContractId = settlement.ContractId,
                PeriodStart = settlement.PeriodStart,
                PeriodEnd = settlement.PeriodEnd,
                ExtraCosts = settlement.ExtraCosts,
                Description = settlement.Description,
                Status = settlement.Status,
                GrossAmount = settlement.GrossAmount,
                RentDeduction = settlement.RentDeduction,
                NetPayout = settlement.NetPayout,
                ConfirmedAt = settlement.ConfirmedAt,
                ConfirmedByDriver = settlement.ConfirmedByDriver,

                // 🔥 FIX HERE: Convert single Contract → List<ContractDto>
                Contracts = settlement.Contract == null
            ? new List<ContractDto>()
            : new List<ContractDto>
            {
                new ContractDto
                {
                    ContractId = settlement.Contract.ContractId,
                    CompanyId = settlement.Contract.CompanyId,
                    DriverId = settlement.Contract.DriverId,
                    DriverName = settlement.Contract.Driver?.DriverName,
                    CarId = settlement.Contract.CarId,
                    CarName = settlement.Contract.Car?.Brand,
                    StartDate = settlement.Contract.StartDate,
                    EndDate = settlement.Contract.EndDate,
                    Status = settlement.Contract.Status,
                    PaymentAmount = settlement.Contract.PaymentAmount,
                    Description = settlement.Contract.Description,
                    Conditions = settlement.Contract.Conditions,
                 
                }
            },

                Earnings = settlement.Earnings.Select(e => new EarningDto
                {
                    EarningId = e.EarningId,
                    ContractId = e.ContractId,
                    SettlementId = e.SettlementId,
                    Platform = e.Platform,
                    GrossIncome = e.GrossIncome,
                    BtwPercentage = e.BtwPercentage,
                    BtwAmount = e.BtwAmount,
                    NetIncome = e.NetIncome,
                    IncomeDate = e.IncomeDate,
                    WeekStart = e.WeekStart,
                    WeekEnd = e.WeekEnd,
                    CompanyId = e.CompanyId
                }).ToList()
            };
        }

    }
    // Helper Result Classes (in shared project if reusable)
    public class ValidationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class LinkResult : ValidationResult
    {
        public List<Earning>? Data { get; set; }  // Linked earnings
    }
}
