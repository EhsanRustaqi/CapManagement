using CapManagement.Client.Pages.Driver.Driver;
using CapManagement.Server.IRepository;
using CapManagement.Server.IService;
using CapManagement.Server.Repository;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.DriverDtoModels;
using CapManagement.Shared.DtoModels.EarningDtoModels;
using CapManagement.Shared.Models;

namespace CapManagement.Server.Services
{
    public class EarningService : IEarningService
    {
        private readonly IEarningRepository _earningRepository;
        private readonly IContractRepository _contractRepository;



        public EarningService(IEarningRepository earningRepository, IContractRepository contractRepository)
        {
            _earningRepository = earningRepository;
            _contractRepository = contractRepository;
        }



        /// <summary>
        /// Archives (soft deletes) an earning record for a specific company.
        /// </summary>
        /// <param name="earningId">The earning ID to archive.</param>
        /// <param name="companyId">The company ID that owns the earning.</param>
        /// <returns>ApiResponse indicating whether the archive was successful.</returns>
        public Task<ApiResponse<bool>> ArchiveEarningtAsync(Guid earningId, Guid companyId)
        {
            throw new NotImplementedException();
        }


        // below create earning method is only for testing only one earning enitity

        public async Task<ApiResponse<EarningDto>> CreateEarningAsync(CreateEarningDto earningDto, string userId)
        {
            var response = new ApiResponse<EarningDto>();

            try
            {
                // 🔹 1. Validate input
                if (earningDto == null)
                {
                    response.Success = false;
                    response.Errors.Add("Earning data cannot be null.");
                    return response;
                }

                if (earningDto.CompanyId == Guid.Empty)
                {
                    response.Success = false;
                    response.Errors.Add("Invalid Company ID.");
                    return response;
                }

                if (earningDto.ContractId == Guid.Empty)
                {
                    response.Success = false;
                    response.Errors.Add("Invalid Contract ID.");
                    return response;
                }

                // 🔹 2. Check if the contract exists and is active
                var contract = await _contractRepository.GetActiveContractAsync(earningDto.ContractId);
                if (contract == null || !contract.IsActive)
                {
                    response.Success = false;
                    response.Errors.Add("The specified contract does not exist or is inactive.");
                    return response;
                }

                // 🔹 3. Validate financials
                if (earningDto.GrossIncome <= 0)
                {
                    response.Success = false;
                    response.Errors.Add("Gross income must be greater than zero.");
                    return response;
                }

                if (earningDto.BtwPercentage < 0 || earningDto.BtwPercentage > 100)
                {
                    response.Success = false;
                    response.Errors.Add("BTW percentage must be between 0 and 100.");
                    return response;
                }




                // ✅ 4. Compute derived financials
                earningDto.BtwAmount = earningDto.GrossIncome * (earningDto.BtwPercentage / 100);
                earningDto.NetIncome = earningDto.GrossIncome - earningDto.BtwAmount;


                // 🔹 4. Map DTO → Domain Model
                var earning = new Earning
                {
                    EarningId = Guid.NewGuid(),
                    CompanyId = earningDto.CompanyId,
                    ContractId = earningDto.ContractId,
                    SettlementId = earningDto.SettlementId,
                    Platform = earningDto.Platform,
                    GrossIncome = earningDto.GrossIncome,
                    BtwPercentage = earningDto.BtwPercentage,

                    IncomeDate = earningDto.IncomeDate,
                    WeekStart = earningDto.WeekStart,
                    WeekEnd = earningDto.WeekEnd,
                
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // 🔹 5. Call repository
                var result = await _earningRepository.CreateEarningAysnc(earning);
                if (result == null)
                {
                    response.Success = false;
                    response.Errors.Add("Failed to create earning record in database.");
                    return response;
                }

                // 🔹 6. Map to response DTO
                var earningDtoResponse = new EarningDto
                {
                    EarningId = earning.EarningId,
                    CompanyId = earning.CompanyId,
                    ContractId = earning.ContractId,
                    SettlementId = earning.SettlementId,
                    Platform = earning.Platform,
                    GrossIncome = earning.GrossIncome,
                    BtwPercentage = earning.BtwPercentage,
                    IncomeDate = earning.IncomeDate,
                    NetIncome = earning.GrossIncome - earning.BtwAmount,  // ✅ directly calculated
                    WeekStart = earning.WeekStart,
                    WeekEnd = earning.WeekEnd,
                    //NetIncome = earning.NetIncome,
                    BtwAmount = earning.BtwAmount
                };

                // 🔹 7. Return success
                response.Success = true;
                response.Message = "Earning created successfully.";
                response.Data = earningDtoResponse;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Errors.Add($"Error creating earning: {ex.Message}");
            }

            return response;


        }




        /// <summary>
        /// Creates a new earning record for a specific contract and company.
        /// </summary>
        /// <param name="earningDto">
        /// The data required to create the earning, including contract, platform, gross income,
        /// VAT (BTW) percentage, and income period.
        /// </param>
        /// <param name="userId">
        /// The ID of the user creating the earning (used for audit tracking).
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{EarningDto}"/> containing the created earning data if successful,
        /// or validation/error messages if the operation fails.
        /// </returns>
        /// <remarks>
        /// This method performs the following:
        /// <list type="bullet">
        /// <item>Validates the input DTO and financial values</item>
        /// <item>Ensures the referenced contract exists, belongs to the company, and is active</item>
        /// <item>Calculates VAT (BTW) and net income</item>
        /// <item>Persists the earning record in the database</item>
        /// </list>
        /// </remarks>
        public async Task<ApiResponse<List<EarningDto>>> CreateEarningsAsync(List<CreateEarningDto> dtos)
        {
            if (!dtos.Any())
            {
                return new ApiResponse<List<EarningDto>> { Success = false, Message = "No earnings to create." };
            }

            // Validate consistency (same company/contract)
            var companyId = dtos.First().CompanyId;
            var contractId = dtos.First().ContractId;
            if (dtos.Any(d => d.CompanyId != companyId || d.ContractId != contractId))
            {
                return new ApiResponse<List<EarningDto>> { Success = false, Message = "All earnings must share the same company/contract." };
            }

            // Optional: Compute derived (BtwAmount, NetIncome)
            foreach (var dto in dtos)
            {
                dto.BtwAmount = dto.GrossIncome * (dto.BtwPercentage / 100m);
                dto.NetIncome = dto.GrossIncome - dto.BtwAmount;
            }

            // Map to entities
            var entities = dtos.Select(dto => new Earning
            {
                CompanyId = dto.CompanyId,
                ContractId = dto.ContractId,
                Platform = dto.Platform,
                WeekStart = dto.WeekStart,
                WeekEnd = dto.WeekEnd,
                GrossIncome = dto.GrossIncome,
                BtwPercentage = dto.BtwPercentage,
                //BtwAmount = dto.BtwAmount,
                //NetIncome = dto.NetIncome,
                IsActive = true
            }).ToList();

            // Batch save via repo
            var savedEntities = await _earningRepository.CreateEarningsAsync(entities);

            // Map to output DTOs (with new IDs)
            var savedDtos = savedEntities.Select(e => new EarningDto
            {
                EarningId = e.EarningId,  // Generated
                                          // ... map other props
            }).ToList();

            return new ApiResponse<List<EarningDto>> { Success = true, Data = savedDtos };
        }

        public async Task<ApiResponse<PagedResponse<EarningDto>>> GetAllEarningAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
        {
            var response = new ApiResponse<PagedResponse<EarningDto>>();

            var pagedResult = await _earningRepository.GetAllEarningsAsync(pageNumber, pageSize, companyId, orderBy, filter);

            var pagedResponse = new PagedResponse<EarningDto>
            {
                Data = pagedResult.Items.Select(e => new EarningDto
                {
                    EarningId = e.EarningId,
                    ContractId = e.ContractId,
                    CompanyId = e.CompanyId,
                    Platform = e.Platform,
                    GrossIncome = e.GrossIncome,
                    BtwPercentage = e.BtwPercentage,
                    IncomeDate = e.IncomeDate,
                    WeekStart = e.WeekStart,
                    WeekEnd = e.WeekEnd,
                    SettlementId = e.SettlementId,
                    BtwAmount = e.BtwAmount,
                    NetIncome = e.NetIncome
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

        public Task<ApiResponse<PagedResponse<EarningDto>>> GetArchivedEarningsAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Retrieves a specific earning record by its ID for a given company.
        /// </summary>
        /// <param name="earningId">
        /// The unique identifier of the earning record.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company that owns the earning.
        /// </param>
        /// <returns>
        /// An <see cref="EarningDto"/> containing the earning details if found; otherwise, <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="earningId"/> or <paramref name="companyId"/> is empty.
        /// </exception>
        /// <remarks>
        /// This method validates the input IDs, fetches the earning from the repository,
        /// and maps the domain entity to a DTO before returning it.
        /// </remarks>
        public async Task<EarningDto> GetEarningByIdAsync(Guid earningId, Guid companyId)
        {
            if (earningId == Guid.Empty)
                throw new ArgumentException("Driver ID cannot be empty.", nameof(earningId));

            if (companyId == Guid.Empty)
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

            // Fetch driver from repository
            var earning = await _earningRepository.GetEarningByIdAsync(earningId, companyId);

            if (earning == null)
                return null;

            // Map entity to DTO
            return new EarningDto
            {
                EarningId = earning.EarningId,
                ContractId = earning.ContractId,
                SettlementId = earning.SettlementId,
                CompanyId = earning.CompanyId,
                Platform = earning.Platform,
                GrossIncome = earning.GrossIncome,
                BtwPercentage = earning.BtwPercentage,
                BtwAmount = earning.BtwAmount,
                NetIncome = earning.NetIncome,
                IncomeDate = earning.IncomeDate,
                WeekStart = earning.WeekStart,
                WeekEnd = earning.WeekEnd
            };

        }









        // restoring the soft deleted earning 
        public Task<ApiResponse<bool>> RestoreEarningAsync(Guid earningId, Guid companyId)
        {
            throw new NotImplementedException();
        }
    }
}
