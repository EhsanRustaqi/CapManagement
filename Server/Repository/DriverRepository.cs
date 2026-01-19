using CapManagement.Client.Pages;
using CapManagement.Server.DbContexts;
using CapManagement.Server.IRepository;
using CapManagement.Shared.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CapManagement.Server.Repository
{
    public class DriverRepository : IDriverRepository
    {
        private readonly FleetDbContext _context;
        public DriverRepository(FleetDbContext context)
        {
            _context = context;
        }






        public async Task<bool> ArchiveDriverAsync(Guid driverId, Guid companyId)
        {


            var driver = await _context.Drivers
                   .FirstOrDefaultAsync(d => d.DriverId == driverId && d.CompanyId == companyId);

            if (driver == null)
                return false;

            driver.IsActive = false;
            driver.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
         
        
        
        public async Task<Driver> CreateDriverAsync(Driver driver)
        {
           
            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync();
            return driver;
        }

       

        public async Task<PageResult<Driver>> GetAllDriversAsync(int pageNumber, int pageSize, Guid companyId)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));
            }

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, Math.Min(pageSize, 100));

            var query = _context.Drivers
                .Where(d => d.CompanyId == companyId && d.IsActive)
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var drivers = await query
                .OrderBy(d => d.DriverName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageResult<Driver>
            {
                Items = drivers,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

        }

        public async Task<PageResult<Driver>> GetArchivedDriversAsync(int pageNumber, int pageSize, Guid companyId)
        {
            var query = _context.Drivers.IgnoreQueryFilters().Where(c => !c.IsActive || c.IsDeleted);

            var items = await query
                .OrderBy(c => c.DriverName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await query.CountAsync();

            return new PageResult<Driver>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<Driver?> GetDriverByIdAsync(Guid driverId, Guid companyId)
        {
            if (driverId == Guid.Empty)
                throw new ArgumentException("Driver ID cannot be empty.", nameof(driverId));

            if (companyId == Guid.Empty)
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

            return await _context.Drivers
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DriverId == driverId
                                       && d.CompanyId == companyId
                                       && d.IsActive);
        }

        public async Task<Driver?> GetDriverByLiecnseNumberAsync(string licenseNumber, Guid companyId)
        {
            if (string.IsNullOrWhiteSpace(licenseNumber))
                throw new ArgumentException("License number cannot be empty.", nameof(licenseNumber));

            if (companyId == Guid.Empty)
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

            return await _context.Drivers
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.LicenseNumber == licenseNumber &&
                    d.CompanyId == companyId &&
                    d.IsActive
                );
        }

        public async Task<Driver?> GetDriverByNameAsync(string firstName, string lastName, Guid companyId)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty.", nameof(firstName));

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

            if (companyId == Guid.Empty)
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

            return await _context.Drivers
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.DriverName == firstName &&
                    d.LastName == lastName &&
                    d.CompanyId == companyId &&
                    d.IsActive
                );
        }




        public async  Task<bool> RestoreDriverAsync(Guid driverId, Guid companyId)
        {
            var company = await _context.Drivers
              .IgnoreQueryFilters() // allow fetching archived/inactive
                .FirstOrDefaultAsync(d => d.DriverId == driverId && d.CompanyId == companyId);

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


        public async Task<bool> UpdateDriverAsync(Driver driver)
        {
            if (driver == null)
            {
                throw new ArgumentNullException(nameof(driver));
            }

            if (driver.CompanyId == Guid.Empty)
            {
                throw new ArgumentException("Company ID cannot be empty.", nameof(driver.CompanyId));
            }

            var existing = await _context.Drivers
                .FirstOrDefaultAsync(d => d.DriverId == driver.DriverId && d.CompanyId == driver.CompanyId && d.IsActive);
            if (existing == null)
            {
                throw new InvalidOperationException("Driver not found, inactive, or does not belong to the specified company.");
            }

            var duplicate = await _context.Drivers
                .Where(d => d.CompanyId == driver.CompanyId && d.DriverId != driver.DriverId && d.IsActive &&
                            d.DriverName == driver.DriverName && d.LastName == driver.LastName)
                .FirstOrDefaultAsync();
            if (duplicate != null)
            {
                throw new InvalidOperationException($"A driver with the name {driver.DriverName} {driver.LastName} already exists for this company.");
            }

            existing.DriverName = driver.DriverName;
            existing.LastName = driver.LastName;
            existing.DateOfBirth = driver.DateOfBirth;
            existing.Phonenumber = driver.Phonenumber;
            existing.LicenseNumber = driver.LicenseNumber;
            existing.UserId = driver.UserId;

            _context.Drivers.Update(existing);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
