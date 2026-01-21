using CapManagement.Server.DbContexts;
using CapManagement.Server.IRepository;
using CapManagement.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace CapManagement.Server.Repository
{
    public class ContractRepository : IContractRepository
    {

        private readonly FleetDbContext _context;



        public ContractRepository(FleetDbContext context)
        {
            _context = context;
        }






        public async Task<bool> ArchiveContractAsync(Guid contractId, Guid companyId)
        {
            if (contractId == Guid.Empty)
            {
                throw new ArgumentException("Contract ID cannot be empty.", nameof(contractId));
            }

            if (companyId == Guid.Empty)
            {
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));
            }

            var contract = await _context.Contracts
                .Where(c => c.ContractId == contractId && c.CompanyId == companyId && c.IsActive)
                .FirstOrDefaultAsync();

            if (contract == null)
            {
                return false;
            }

            contract.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }





        public async Task<Contract> CreateContractAsync(Contract contract)
        {
            contract.ContractId = Guid.NewGuid();
            contract.IsActive = true;
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task<Contract?> GetActiveContractAsync(Guid contractId)
        {

            return await _context.Contracts
                .FirstOrDefaultAsync(c => c.ContractId == contractId && c.IsActive);
         
        }

        public async Task<Contract?> GetActiveContractByCarAsync(Guid carId, Guid companyId)
        {
            return await _context.Contracts
        .Where(c => c.CarId == carId
                    && c.CompanyId == companyId
                    && c.Status == ContractStatus.Active)
        .FirstOrDefaultAsync();
        }

        public async Task<PageResult<Contract>> GetAllContractAsync(
       int pageNumber,
       int pageSize,
       Guid companyId,
       string? orderBy = null,
       string? filter = null)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));
            }

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, Math.Min(pageSize, 100));



            var query = _context.Contracts
                .Where(c => c.CompanyId == companyId && c.IsActive)
                .Include(c => c.Driver)
                .Include(c => c.Car)
                .AsNoTracking();

            // ✅ Properly structured: Include Car/Driver if needed for filter or orderBy
            if (!string.IsNullOrEmpty(filter) && (filter.Contains("Car.") || filter.Contains("Driver.")))
            {
                query = query.Include(c => c.Car).Include(c => c.Driver);
            }
            else if (!string.IsNullOrEmpty(orderBy) && (orderBy.Contains("Car.") || orderBy.Contains("Driver.")))
            {
                query = query.Include(c => c.Car).Include(c => c.Driver);
            }

            // Apply filtering (exclude PdfContent)
            if (!string.IsNullOrEmpty(filter) && !filter.Contains("PdfContent"))
            {
                try
                {
                    query = query.Where(filter); // requires System.Linq.Dynamic.Core
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Filter error: {ex.Message}");
                    return new PageResult<Contract>
                    {
                        Items = new List<Contract>(),
                        TotalCount = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }

            // Apply sorting (exclude PdfContent)
            if (!string.IsNullOrEmpty(orderBy) && !orderBy.Contains("PdfContent"))
            {
                try
                {
                    query = query.OrderBy(orderBy); // requires System.Linq.Dynamic.Core
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Sort error: {ex.Message}");
                    query = query.OrderBy(c => c.ContractId);
                }
            }
            else
            {
                query = query.OrderBy(c => c.ContractId);
            }

            var totalCount = await query.CountAsync();
            var contracts = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageResult<Contract>
            {
                Items = contracts,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }






        public async Task<PageResult<Contract>> GetArchivedContractAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
        {
            {
                if (companyId == Guid.Empty)
                {
                    throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));
                }

                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Max(1, Math.Min(pageSize, 100));

                var query = _context.Contracts
                    .Where(c => c.CompanyId == companyId && !c.IsActive) // archived contracts
                    .AsNoTracking();

                // Include Car and Driver if filter/orderBy references them
                if (!string.IsNullOrEmpty(filter) && (filter.Contains("Car.") || filter.Contains("Driver.")))
                {
                    query = query.Include(c => c.Car).Include(c => c.Driver);
                }
                else if (!string.IsNullOrEmpty(orderBy) && (orderBy.Contains("Car.") || orderBy.Contains("Driver.")))
                {
                    query = query.Include(c => c.Car).Include(c => c.Driver);
                }

                // Apply filtering
                if (!string.IsNullOrEmpty(filter) && !filter.Contains("PdfContent"))
                {
                    try
                    {
                        query = query.Where(filter); // requires System.Linq.Dynamic.Core
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Filter error: {ex.Message}");
                        return new PageResult<Contract>
                        {
                            Items = new List<Contract>(),
                            TotalCount = 0,
                            PageNumber = pageNumber,
                            PageSize = pageSize
                        };
                    }
                }

                // Apply sorting
                if (!string.IsNullOrEmpty(orderBy) && !orderBy.Contains("PdfContent"))
                {
                    try
                    {
                        query = query.OrderBy(orderBy); // requires System.Linq.Dynamic.Core
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Sort error: {ex.Message}");
                        query = query.OrderBy(c => c.ContractId);
                    }
                }
                else
                {
                    query = query.OrderBy(c => c.ContractId);
                }

                var totalCount = await query.CountAsync();
                var contracts = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PageResult<Contract>
                {
                    Items = contracts,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }

        }



        public async Task<Contract?> GetContractByCarAsync(string numberPlate, Guid companyId)
        {
            if (string.IsNullOrWhiteSpace(numberPlate))
            {
                throw new ArgumentException("Number plate cannot be empty.", nameof(numberPlate));
            }

            if (companyId == Guid.Empty)
            {
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));
            }

            return await _context.Contracts
                .Include(c => c.Car)
                .Include(c => c.Driver)
                .Where(c => c.Car.NumberPlate == numberPlate && c.CompanyId == companyId && c.IsActive && c.Status == ContractStatus.Active)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }





        public async  Task<Contract?> GetContractByIdAsync(Guid contractId, Guid companyId)
        {
            if (contractId == Guid.Empty)
            {
                throw new ArgumentException("Contract ID cannot be empty.", nameof(contractId));
            }

            if (companyId == Guid.Empty)
            {
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));
            }

            return await _context.Contracts
                .Include(c => c.Car)
                .Include(c => c.Driver)
                .Where(c => c.ContractId == contractId && c.CompanyId == companyId && c.IsActive)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<byte[]?> GetContractPdfAsync(Guid contractId, Guid companyId)
        {
            return await _context.Contracts
      .Where(c => c.ContractId == contractId && c.CompanyId == companyId && c.IsActive)
      .Select(c => c.PdfContent)
      .FirstOrDefaultAsync();
        }

        public async Task<bool> RestoreContractAsync(Guid contractId, Guid companyId)
        {
            if (contractId == Guid.Empty)
            {
                throw new ArgumentException("Contract ID cannot be empty.", nameof(contractId));
            }

            if (companyId == Guid.Empty)
            {
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));
            }

            var contract = await _context.Contracts
                .Where(c => c.ContractId == contractId && c.CompanyId == companyId && !c.IsActive)
                .FirstOrDefaultAsync();

            if (contract == null)
            {
                return false;
            }

            contract.IsActive = true;
            await _context.SaveChangesAsync();

            return true;
        }




        public async  Task<bool> UpdateContractAsync(Contract contract)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            var existingContract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.ContractId == contract.ContractId && c.CompanyId == contract.CompanyId && c.IsActive);

            if (existingContract == null) return false;

            // Update fields (no business validation here)
            existingContract.DriverId = contract.DriverId;
            existingContract.CarId = contract.CarId;
            existingContract.StartDate = contract.StartDate;
            existingContract.EndDate = contract.EndDate;
            existingContract.Status = contract.Status;
            existingContract.PaymentAmount = contract.PaymentAmount;
            existingContract.Description = contract.Description;
            existingContract.Conditions = contract.Conditions;
            existingContract.PdfContent = contract.PdfContent;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateContractStatusAsync(Guid contractId, Guid companyId, ContractStatus newStatus)
        {
            if (contractId == Guid.Empty)
            {
                throw new ArgumentException("Contract ID cannot be empty.", nameof(contractId));
            }

            if (companyId == Guid.Empty)
            {
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));
            }

            var contract = await _context.Contracts
                .Where(c => c.ContractId == contractId && c.CompanyId == companyId && c.IsActive)
                .FirstOrDefaultAsync();

            if (contract == null)
            {
                return false;
            }

            contract.Status = newStatus;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
