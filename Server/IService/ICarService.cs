using CapManagement.Shared.DtoModels.DriverDtoModels;
using CapManagement.Shared.Models;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.CarDtoModels;

namespace CapManagement.Server.IService
{
    public interface ICarService
    {

        Task<ApiResponse<PagedResponse<CarDto>>> GetAllCarAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter);

        Task<ApiResponse<CarDto>> CreateCarAsync(CreateCarDto carDto, string userId);



        Task<CarDto> GetCarByIdAsync(Guid carId, Guid companyId);

        Task<ApiResponse<bool>> ArchiveCarAsync(Guid carId, Guid companyId);

        Task<ApiResponse<bool>> RestoreCarAsync(Guid carId, Guid companyId);

        Task<ApiResponse<bool>> UpdateCarAsync(CarDto carDto);

        Task<ApiResponse<PagedResponse<CarDto>>> GetArchivedCarsAsync(int pageNumber, int pageSize, Guid companyId);


        Task<ApiResponse<List<CarDto>>> GetCArsWithoutActiveContractAsync(Guid companyId);

    }
}
