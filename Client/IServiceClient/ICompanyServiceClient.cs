using CapManagement.Shared.DtoModels.CompanyDtoModels;
using CapManagement.Shared.Models;
using CapManagement.Shared;

namespace CapManagement.Client.IServiceClient
{
    public interface ICompanyServiceClient
    {
        Task<ApiResponse<PagedResponse<CompanyDto>>> GetCompaniesAsync(int? pageNumber = null, int? pageSize = null);

        //Task<PagedResponse<CompanyDto>> GetCompaniesAsync(int? pageNumber = null, int? pageSize = null);
        Task<ApiResponse<CompanyDto>> CreateCompanyAsync(CreateCompanyDto dto, string userId);



        Task<ApiResponse<bool>> ArchiveCompanyAsync(Guid companyId);


        Task<ApiResponse<bool>> RestoreCompanyAsync(Guid companyId);




        Task<ApiResponse<PagedResponse<CompanyDto>>> GetArchivedCompaniesAsync(int pageNumber, int pageSize);

        Task<ApiResponse<CompanyDto>> GetCompanyByIdAsync(Guid companyId);

        Task<ApiResponse<bool>> UpdateCompanyAsync(CompanyDto company);
    };
}
