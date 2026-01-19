using CapManagement.Shared.DtoModels.CompanyDtoModels;
using CapManagement.Shared.DtoModels;
using CapManagement.Shared.Models;
using CapManagement.Shared;

namespace CapManagement.Server.IRepository
{
    public interface ICompanyRepository
    {
        // Create a new company
   Task <Company> CreateCompanyAsync(Company company);
        

        Task<bool> UpdateCompanyAsync(Company company);

        Task<bool> ArchiveCompanyAsync(Guid companyId);


         Task <bool> RestoreCompanyAsync(Guid companyId);

        // Update existing company
        

        // Soft-delete (archive) a company
        // Get company by ID
        Task<Company> GetCompanyByIdAsync(Guid companyId);


        // Get all active companies with pagination
        Task<PageResult<Company>> GetAllCompanyAsync(int pageNumber, int pageSize);


        // Get archived companies with pagination
        Task<PageResult<Company>> GetArchivedCompanyAsync(int pageNumber, int pageSize);
        // Get company by VAT number
        Task<Company> GetCompanyByVatNumberAsync(string vatNumber);

        // Get company by name
        Task<Company> GetCompanyByNameAsync(string companyName);

    }
}
