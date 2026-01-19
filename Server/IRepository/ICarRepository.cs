using CapManagement.Shared.Models;

namespace CapManagement.Server.IRepository
{
    public interface ICarRepository
    {
        //Task<PageResult<Car>> GetAllCarAsync(int pageNumber, int pageSize, Guid companyId);

        Task<PageResult<Car>> GetAllCarAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter);


        Task<PageResult<Car>> GetArchivedCarAsync(int pageNumber, int pageSize, Guid companyId);


        Task <Car> CreateCarAsync(Car car);

        Task<bool> UpdateCarAsync(Car car);

        Task<bool> ArchiveCarAsync(Guid carId, Guid companyId);

        Task<bool> RestoreCarAsync(Guid carId, Guid companyId);


        Task<Car?> GetCarByIdAsync(Guid carId, Guid companyId);

        Task<Car?> GetCarByNumberPlateAsync(string carName, Guid companyId
            );

        Task<Car?> GetCarBasicInfoAsync(Guid carId, Guid companyId);

        Task<Car?> GetCarWithContractsAsync(Guid carId, Guid companyId);
    }
}
