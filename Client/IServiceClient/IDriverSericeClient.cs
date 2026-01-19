using CapManagement.Shared.DtoModels.CompanyDtoModels;
using CapManagement.Shared.Models;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.DriverDtoModels;

namespace CapManagement.Client.IServiceClient
{
    public interface IDriverSericeClient
    {
        Task<ApiResponse<PagedResponse<DriverDto>>> GetDriversAsync(Guid companyId, int pageNumber = 1, int pageSize = 10);

        Task<ApiResponse<DriverDto>> CreateDriverAsync(CreateDriverDto dto);

        Task<ApiResponse<bool>> ArchiveDriverAsync(Guid driverId, Guid companyId);

        Task<ApiResponse<bool>> RestoreDriverAsync(Guid driverId, Guid companyId);

        Task<ApiResponse<PagedResponse<DriverDto>>> GetArchivedDriversAsync(Guid companyId, int pageNumber = 1, int pageSize = 10);

        Task<ApiResponse<DriverDto>> GetDriverByIdAsync(Guid driverId, Guid companyId);

        Task<ApiResponse<bool>> UpdateDriverAsync(DriverDto driver);


    }
}
