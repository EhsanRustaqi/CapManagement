using CapManagement.Client.Pages.Driver.Driver;
using CapManagement.Server.DbContexts;
using CapManagement.Server.IRepository;
using CapManagement.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
namespace CapManagement.Server.Repository
{
    public class CarRepository : ICarRepository
    {
        private readonly FleetDbContext _context;
        public CarRepository(FleetDbContext context)
        {
            _context = context;
        }
        public async Task<bool> ArchiveCarAsync(Guid carId, Guid companyId)
        {

            var car = await _context.Cars
                   .FirstOrDefaultAsync(d => d.CarId == carId && d.CompanyId == companyId);

            if (car == null)
                return false;

            car.IsActive = false;
            car.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }





        public async Task<Car> CreateCarAsync(Car car)
        {
            _context.Cars.Add(car);
            await _context.SaveChangesAsync();
            return car;
        }





        //public async Task<PageResult<Car>> GetAllCarAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
        //{
        //    if (companyId == Guid.Empty)
        //    {
        //        throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));
        //    }

        //    pageNumber = Math.Max(1, pageNumber);
        //    pageSize = Math.Max(1, Math.Min(pageSize, 100));

        //    var query = _context.Cars
        //        .Where(d => d.CompanyId == companyId && d.IsActive)
        //        .AsNoTracking();


           



        //    var totalCount = await query.CountAsync();
        //    var cars = await query
        //        .OrderBy(d => d.Brand)
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();

        //    return new PageResult<Car>
        //    {
        //        Items = cars,
        //        TotalCount = totalCount,
        //        PageNumber = pageNumber,
        //        PageSize = pageSize
        //    };
        //}
       

public async Task<PageResult<Car>> GetAllCarAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));
        }

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, Math.Min(pageSize, 100));

        var query = _context.Cars
            .Where(d => d.CompanyId == companyId && d.IsActive)
            .AsNoTracking();

        // Apply filtering
        if (!string.IsNullOrEmpty(filter))
        {
            try
            {
                query = query.Where(filter);
            }
            catch (Exception ex)
            {
                // Log the error and return empty result to prevent crashing
                Console.WriteLine($"Filter error: {ex.Message}");
                return new PageResult<Car>
                {
                    Items = new List<Car>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(orderBy))
        {
            try
            {
                query = query.OrderBy(orderBy);
            }
            catch (Exception ex)
            {
                // Log the error and fall back to default sort
                Console.WriteLine($"Sort error: {ex.Message}");
                query = query.OrderBy(c => c.CarId); // Default sort
            }
        }
        else
        {
            query = query.OrderBy(c => c.CarId); // Default sort by CarId
        }

        // Remove redundant OrderBy(d => d.Brand)
        var totalCount = await query.CountAsync();
        var cars = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<Car>
        {
            Items = cars,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }







    public async Task<PageResult<Car>> GetArchivedCarAsync(int pageNumber, int pageSize, Guid companyId)
        {
            var query = _context.Cars.IgnoreQueryFilters().Where(c => !c.IsActive || c.IsDeleted);

            var items = await query
                .OrderBy(c => c.Brand)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await query.CountAsync();

            return new PageResult<Car>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            }; ;
        }




        public async Task<Car?> GetCarBasicInfoAsync(Guid carId, Guid companyId)
        {
            return await _context.Cars
      .Where(c => c.CarId == carId && c.CompanyId == companyId && c.IsActive)
      .Select(c => new Car
      {
          CarId = c.CarId,
          CompanyId = c.CompanyId,
          NumberPlate = c.NumberPlate,
          IsActive = c.IsActive
      })
      .FirstOrDefaultAsync();
        }

        public async Task<Car?> GetCarByIdAsync(Guid carId, Guid companyId)
        {
            if (carId == Guid.Empty)
                throw new ArgumentException("Driver ID cannot be empty.", nameof(carId));

            if (companyId == Guid.Empty)
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

            return await _context.Cars
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.CarId == carId
                                       && d.CompanyId == companyId
                                       && d.IsActive);
            
        }



        public async Task<Car?> GetCarByNumberPlateAsync(string numberPlate, Guid companyId)
        {
            if (string.IsNullOrWhiteSpace(numberPlate))
                throw new ArgumentException("License number cannot be empty.", nameof(numberPlate));

            if (companyId == Guid.Empty)
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

            return await _context.Cars
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.NumberPlate == numberPlate &&
                    d.CompanyId == companyId &&
                    d.IsActive
                );
        }

        public async Task<Car?> GetCarWithContractsAsync(Guid carId, Guid companyId)
        {
            return await _context.Cars
         .Include(c => c.Contracts)
         .FirstOrDefaultAsync(c => c.CarId == carId && c.CompanyId == companyId);
        }

        public async Task<bool> RestoreCarAsync(Guid carId, Guid companyId)
        {
            var company = await _context.Cars
              .IgnoreQueryFilters() // allow fetching archived/inactive
                .FirstOrDefaultAsync(d => d.CarId == carId && d.CompanyId == companyId);

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

        public async Task<bool> UpdateCarAsync(Car car)
        {
            if (car == null)
            {
                throw new ArgumentNullException(nameof(car));
            }

            if (car.CompanyId == Guid.Empty)
            {
                throw new ArgumentException("Company ID cannot be empty.", nameof(car.CompanyId));
            }

            // Fetch the existing active car for the given company
            var existing = await _context.Cars
                .FirstOrDefaultAsync(c => c.CarId == car.CarId && c.CompanyId == car.CompanyId && c.IsActive);

            if (existing == null)
            {
                throw new InvalidOperationException("Car not found, inactive, or does not belong to the specified company.");
            }

            // Check for duplicate NumberPlate within the same company
            var duplicate = await _context.Cars
                .Where(c => c.CompanyId == car.CompanyId && c.CarId != car.CarId && c.IsActive &&
                            c.NumberPlate == car.NumberPlate)
                .FirstOrDefaultAsync();

            if (duplicate != null)
            {
                throw new InvalidOperationException($"A car with the number plate '{car.NumberPlate}' already exists for this company.");
            }

            // Update fields
            existing.Brand = car.Brand;
            existing.Model = car.Model;
            existing.NumberPlate = car.NumberPlate;
            existing.Year = car.Year;
            existing.Status = car.Status;

            _context.Cars.Update(existing);
            return await _context.SaveChangesAsync() > 0;
        }

    }
}
