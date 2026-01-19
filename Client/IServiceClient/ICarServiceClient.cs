using CapManagement.Shared.DtoModels.CarDtoModels;
using CapManagement.Shared.DtoModels.DriverDtoModels;
using CapManagement.Shared.Models;
using CapManagement.Shared;

namespace CapManagement.Client.IServiceClient
{
    public interface ICarServiceClient  
    {
        //Task<ApiResponse<PagedResponse<CarDto>>> GetCarsAsync(Guid companyId, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<PagedResponse<CarDto>>> GetCarsAsync(Guid companyId, int pageNumber = 1, int pageSize = 10, string? orderBy = null, string? filter = null);
        Task<ApiResponse<CarDto>> CreateCarsAsync(CreateCarDto dto);

        Task<ApiResponse<bool>> ArchiveCarAsync(Guid carId, Guid companyId);

        Task<ApiResponse<bool>> RestoreCarAsync(Guid carId, Guid companyId);

        Task<ApiResponse<PagedResponse<CarDto>>> GetArchivedCarsAsync(Guid companyId, int pageNumber = 1, int pageSize = 10);

        Task<ApiResponse<CarDto>> GetCarByIdAsync(Guid CarId, Guid companyId);

        Task<ApiResponse<bool>> UpdateCarAsync(CarDto driver);

        Task<CarDto?> GetCarInfoFromRdwAsync(string numberPlate);

    }
}
