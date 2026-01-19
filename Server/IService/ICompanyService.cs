using CapManagement.Shared.DtoModels.CompanyDtoModels;
using CapManagement.Shared.Models;
using CapManagement.Shared;

namespace CapManagement.Server.IService
{
    public interface ICompanyService
    {

        Task<ApiResponse<PagedResponse<CompanyDto>>> GetAllCompanyAsync(int pageNumber, int pageSize);

        Task<ApiResponse<CompanyDto>> CreateCompanyAsync(CreateCompanyDto companyDto, string userId);



        Task<CompanyDto> GetCompanyByIdAsync(Guid companyId);

        Task<ApiResponse<bool>> ArchiveCompanyAsync(Guid companyId);

        Task<ApiResponse<bool>> RestoreCompanyAsync(Guid companyId);

        Task<ApiResponse<bool>> UpdateCompanyAsync(CompanyDto companyDto);

        Task<ApiResponse<PagedResponse<CompanyDto>>> GetArchivedCompaniesAsync(int pageNumber, int pageSize);
    }
}
