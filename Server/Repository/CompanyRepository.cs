using CapManagement.Server.DbContexts;
using CapManagement.Server.IRepository;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels;
using CapManagement.Shared.DtoModels.CompanyDtoModels;
using CapManagement.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CapManagement.Server.Repository
{


    public class CompanyRepository : ICompanyRepository
    {
        private readonly FleetDbContext _context;
        public CompanyRepository(FleetDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ArchiveCompanyAsync(Guid companyId)
        {
            var company = await _context.Company.FindAsync(companyId);
            if (company == null) return false;

            company.IsActive = false;
            company.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
          
        }

        public async Task<Company> CreateCompanyAsync(Company company)
        {
            _context.Company.Add(company);    
            await _context.SaveChangesAsync();
            return company;
        }



        public async Task<PageResult<Company>> GetAllCompanyAsync(int pageNumber, int pageSize)
        {
            var query = _context.Company.Where(c => c.IsActive);

            var items = await query
                .OrderBy(c => c.CompanyName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();   // ✅ ToList on the collection, not a single Company

            var totalCount = await query.CountAsync();

            return new PageResult<Company>
            {
                Items = items,                // ✅ pass list of companies
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }




        public async Task<PageResult<Company>> GetArchivedCompanyAsync(int pageNumber, int pageSize)
        {
            //      //var query = _context.Company.Where(c => !c.IsActive);
            //      var query = _context.Company
            //.IgnoreQueryFilters() // 🚀 this disables the global "IsActive" filter
            //.Where(c => !c.IsActive && c.DeletedAt != null)
            //.OrderBy(c => c.CompanyName);
            //      var items = await query
            //          .OrderBy(c => c.CompanyName)
            //          .Skip((pageNumber - 1) * pageSize)
            //          .Take(pageSize)
            //          .ToListAsync();   // ✅ ToList on the collection, not a single Company

            //      var totalCount = await query.CountAsync();

            //      return new PageResult<Company>
            //      {
            //          Items = items,                // ✅ pass list of companies
            //          TotalCount = totalCount,
            //          PageNumber = pageNumber,
            //          PageSize = pageSize
            //      };

            var query = _context.Company
       .IgnoreQueryFilters() // 🚀 disables global filters
       .Where(c => !c.IsActive || c.IsDeleted); // archived = inactive or marked deleted

            var items = await query
                .OrderBy(c => c.CompanyName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await query.CountAsync();

            return new PageResult<Company>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

        }



        public async Task<Company> GetCompanyByIdAsync(Guid companyId)
        {
            return await _context.Company
                 .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.IsActive);
        }


        public async Task<Company> GetCompanyByNameAsync(string companyName)
        {
            return await _context.Company
                 .FirstOrDefaultAsync(c => c.CompanyName.ToLower() == companyName.ToLower() && c.IsActive);
        }

        public async Task<Company> GetCompanyByVatNumberAsync(string vatNumber)
        {
            var result = await _context.Company.FirstOrDefaultAsync(c => c.VATNumber == vatNumber && c.IsActive);
            return result;
        }

        public async Task<bool> RestoreCompanyAsync(Guid companyId)
        {
            var company = await _context.Company
        .IgnoreQueryFilters() // allow fetching archived/inactive
        .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            if (company == null)
            {
                Console.WriteLine($"No company found with ID {companyId}");
                return false;
            }

            Console.WriteLine($"Restoring company {company.CompanyId}, WasActive: {company.IsActive}, DeletedAt: {company.DeletedAt}");

            // Restore flags
            company.IsActive = true;
            company.IsDeleted = false;    // ✅ ensure global filters pick it up again
            company.DeletedAt = null;
            company.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"Restored company {company.CompanyId}, IsActive: {company.IsActive}, IsDeleted: {company.IsDeleted}, DeletedAt: {company.DeletedAt}");
                return true;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error restoring company: {ex.InnerException?.Message ?? ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCompanyAsync(Company company)
        {
            _context.Company.Update(company);
           return await _context.SaveChangesAsync() > 0;
            
        }
    }
}
