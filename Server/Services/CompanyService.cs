using CapManagement.Server.IRepository;
using CapManagement.Server.IService;
using CapManagement.Server.Repository;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.CompanyDtoModels;
using CapManagement.Shared.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CapManagement.Server.Services
{
    public class CompanyService : ICompanyService
    {

        private readonly ICompanyRepository _companyRepository;
        public CompanyService(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }





        /// <summary>
        /// Archives a company by its identifier.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company to archive.</param>
        /// <returns>
         /// An ApiResponse indicating whether the archive operation succeeded.
        public async Task<ApiResponse<bool>> ArchiveCompanyAsync(Guid companyId)
        {
            var response = new ApiResponse<bool>();

            try
            {
              
                // Business rule: a company must exist before it can be archived
                var company = await _companyRepository.GetCompanyByIdAsync(companyId);
                if (company == null)
                {
                    response.Success = false;
                    response.Errors = new List<string> { "Company not found." };
                    return response;
                }

                // Delegate the archive operation to the repository
                var result = await _companyRepository.ArchiveCompanyAsync(companyId);

                response.Success = result;
                response.Data = result;


                // Handle failure case when the archive operation does not succeed
                if (!result)
                {
                    response.Errors = new List<string> { "Failed to archive the company." };
                }
            }
            catch (Exception ex)
            {

                // Catch unexpected errors and return a safe, consistent response
                response.Success = false;
                response.Errors = new List<string> { "An error occurred while archiving the company.", ex.Message };
            }

            return response;
        }






        /// <summary>
        /// Creates a new company after validating input and enforcing uniqueness rules.
        /// </summary>
        /// <param name="companyDto">DTO containing company creation data.</param>
        /// <param name="userId">Identifier of the user creating the company.</param>
        /// <returns>
        /// An ApiResponse containing the created company data or validation errors.
        /// </returns>

        public async Task<ApiResponse<CompanyDto>> CreateCompanyAsync(CreateCompanyDto companyDto, string userId)
        {
           var response = new ApiResponse<CompanyDto>();


            // Basic validation: company name is mandatory
            if (string.IsNullOrWhiteSpace(companyDto.CompanyName))
            {
                response.Success = false;
                response.Errors = new List<string> { "Company name is required." };
                return response;
            }

            // ===== Check for duplicates =====
            var existingByName = await _companyRepository.GetCompanyByNameAsync(companyDto.CompanyName);
            if (existingByName != null)
            {
                response.Success = false;
                response.Errors = new List<string> { "A company with this name already exists." };
                return response;
            }

            // Business rule: VAT number, if provided, must also be unique
            if (!string.IsNullOrWhiteSpace(companyDto.VATNumber))
            {
                var existingByVAT = await _companyRepository.GetCompanyByVatNumberAsync(companyDto.VATNumber);
                if (existingByVAT != null)
                {
                    response.Success = false;
                    response.Errors = new List<string> { "A company with this VAT number already exists." };


                    return response;
                }
            }

            // Map DTO to domain entity and set system-managed fields

            var company = new Company
            {
                CompanyId = Guid.NewGuid(),
                CompanyName = companyDto.CompanyName,
                Address = companyDto.Address,
                VATNumber = companyDto.VATNumber,
                CompanyEmail = companyDto.CompanyEmail,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedByUserId = Guid.Parse(userId)
            };


            try
            {

                // Persist the new company
                var createdCompany = await _companyRepository.CreateCompanyAsync(company);

                // Map domain entity to response DTO
                var companyDtoResponse = new CompanyDto
                {
                    CompanyId = createdCompany.CompanyId,
                    CompanyName = createdCompany.CompanyName,
                    Address = createdCompany.Address,
                    VATNumber = createdCompany.VATNumber,
                    CreatedAt = createdCompany.CreatedAt,
                    IsActive = createdCompany.IsActive
                };

                // Handle database-level failures (e.g., constraint violations)
                response.Success = true;
                response.Data = companyDtoResponse;
            }
            catch (DbUpdateException ex)
            {
                response.Success = false;
                response.Errors = new List<string> { "Company name is required." };
                // Log exception
            }

            return response;
        }


        /// <summary>
        /// Retrieves a paginated list of all companies in the system.
        /// </summary>
        /// <param name="pageNumber">
        /// The page number to retrieve (starting from 1).
        /// </param>
        /// <param name="pageSize">
        /// The number of companies to include per page.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a paginated list of companies,
        /// including pagination metadata such as total pages and total items.
        /// </returns>
        public async Task<ApiResponse<PagedResponse<CompanyDto>>> GetAllCompanyAsync(int pageNumber, int pageSize)
        {
            var response = new ApiResponse<PagedResponse<CompanyDto>>();
            var pagedResult = await _companyRepository.GetAllCompanyAsync(pageNumber, pageSize);

            var pagedResponse = new PagedResponse<CompanyDto>
            {
                Data = pagedResult.Items.Select(c => new CompanyDto
                {
                    CompanyId = c.CompanyId,
                    CompanyName = c.CompanyName,
                    Address = c.Address,
                    VATNumber = c.VATNumber,
                    CompanyEmail = c.CompanyEmail,
                    CreatedAt = c.CreatedAt,
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
        /// Retrieves a paginated list of archived (inactive) companies.
        /// Archived companies are no longer active but are retained for historical,
        /// reporting, or audit purposes.
        /// </summary>
        /// <param name="pageNumber">
        /// The page number to retrieve (starting from 1).
        /// </param>
        /// <param name="pageSize">
        /// The number of companies to include per page.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing a paginated list of archived companies,
        /// including pagination metadata such as total pages and total items.
        /// </returns>
        public async Task<ApiResponse<PagedResponse<CompanyDto>>> GetArchivedCompaniesAsync(int pageNumber, int pageSize)
        {
            var response = new ApiResponse<PagedResponse<CompanyDto>>();
            var pagedResult = await _companyRepository.GetArchivedCompanyAsync(pageNumber, pageSize);

            var pagedResponse = new PagedResponse<CompanyDto>
            {
                Data = pagedResult.Items.Select(c => new CompanyDto
                {
                    CompanyId = c.CompanyId,
                    CompanyName = c.CompanyName,
                    Address = c.Address,
                    VATNumber = c.VATNumber,
                    CompanyEmail = c.CompanyEmail,
                    CreatedAt = c.CreatedAt,
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
        /// Retrieves a single company by its unique identifier.
        /// </summary>
        /// <param name="companyId">
        /// The unique identifier of the company to retrieve.
        /// </param>
        /// <returns>
        /// A <see cref="CompanyDto"/> if the company is found; otherwise, <c>null</c>.
        /// The controller can translate a null result into a 404 Not Found response.
        /// </returns>
        public async Task<CompanyDto> GetCompanyByIdAsync(Guid companyId)
        {
          var company = await _companyRepository.GetCompanyByIdAsync(companyId);

    if (company == null)
        return null; // Controller can return 404 Not Found

    return new CompanyDto
    {
        CompanyId = company.CompanyId,
        CompanyName = company.CompanyName,
        Address = company.Address,
        VATNumber = company.VATNumber,
        CompanyEmail = company.CompanyEmail,
        CreatedAt = company.CreatedAt,
        IsActive = company.IsActive
    };
        }




        /// <summary>
        /// Restores a previously archived company and marks it as active again.
        /// </summary>
        /// <param name="companyId">
        /// The unique identifier of the company to restore.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating whether the restore operation was successful.
        /// </returns>
        public async Task<ApiResponse<bool>> RestoreCompanyAsync(Guid companyId)
        {
            var response = new ApiResponse<bool>();

            try
            {
                var result = await _companyRepository.RestoreCompanyAsync(companyId);

                if (!result)
                {
                    response.Success = false;
                    response.Errors = new List<string> { "Company not found or failed to restore." };
                    return response;
                }

                response.Success = true;
                response.Data = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Errors = new List<string> { "An error occurred while restoring the company.", ex.Message };
            }

            return response;
        }



        /// <summary>
        /// Updates an existing company's information after validating input and business rules.
        /// </summary>
        /// <param name="companyDto">
        /// The data transfer object containing the updated company information.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating whether the update operation was successful.
        /// </returns>
        public async Task<ApiResponse<bool>> UpdateCompanyAsync(CompanyDto companyDto)
        {
            try
            {
                if (companyDto == null || companyDto.CompanyId == Guid.Empty)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Errors = new List<string> { "Invalid company data or ID." }
                    };
                }

                var company = await _companyRepository.GetCompanyByIdAsync(companyDto.CompanyId);
                if (company == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Errors = new List<string> { $"Company with ID {companyDto.CompanyId} not found." }
                    };
                }

                // Map DTO to entity
                company.CompanyName = companyDto.CompanyName;
                company.VATNumber = companyDto.VATNumber;
                company.CompanyEmail = companyDto.CompanyEmail;
                company.Address = companyDto.Address;
                company.IsActive = companyDto.IsActive;
                company.UpdatedAt = DateTime.UtcNow; // Optional: Remove if UpdatedAt isn’t in your model

                var success = await _companyRepository.UpdateCompanyAsync(company);
                if (!success)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Errors = new List<string> { "Failed to update company in the database." }
                    };
                }

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Errors = new List<string>()
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { $"Error updating company: {ex.Message}" }
                };
            }
        }
    }
};
