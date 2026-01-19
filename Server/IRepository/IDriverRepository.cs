using CapManagement.Shared;
using CapManagement.Shared.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CapManagement.Server.IRepository
{
    public interface IDriverRepository
    {

        Task<PageResult<Driver>> GetAllDriversAsync(int pageNumber, int pageSize, Guid companyId);
        Task<PageResult<Driver>> GetArchivedDriversAsync(int pageNumber, int pageSize, Guid companyId);
        Task<Driver> CreateDriverAsync(Driver driver);
        Task<bool> UpdateDriverAsync(Driver driver);
        Task <bool>ArchiveDriverAsync(Guid driverId, Guid companyId);
        Task <bool>RestoreDriverAsync(Guid driverId, Guid companyId);
        Task<Driver?> GetDriverByIdAsync(Guid driverId, Guid companyId);
        Task<Driver?> GetDriverByNameAsync(string firstName, string lastName, Guid companyId);

        Task<Driver?> GetDriverByLiecnseNumberAsync(string licenseNumber, Guid companyId);




    }
}
