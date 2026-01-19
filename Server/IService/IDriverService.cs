using CapManagement.Shared.DtoModels.CompanyDtoModels;
using CapManagement.Shared.Models;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.DriverDtoModels;

namespace CapManagement.Server.IService
{
    public interface IDriverService
    {
        Task<ApiResponse<PagedResponse<DriverDto>>> GetAllDriverAsync(int pageNumber, int pageSize, Guid companyId);

        Task<ApiResponse<DriverDto>> CreateDriverAsync(CreateDriverDto driverDto, string userId);



        Task<DriverDto> GetDriverByIdAsync(Guid driverId, Guid companyId);

        Task<ApiResponse<bool>> ArchiveDriverAsync(Guid driverId, Guid companyId);

        Task<ApiResponse<bool>> RestoreDriverAsync(Guid driverId, Guid companyId);

        Task<ApiResponse<bool>> UpdateDriverAsync(DriverDto driverDto);

        Task<ApiResponse<PagedResponse<DriverDto>>> GetArchivedDriversAsync(int pageNumber, int pageSize, Guid companyId);

    }
}
