using CapManagement.Shared.DtoModels.LoginDto;

namespace CapManagement.Server.IService
{
    public interface IAuthService
    {


        Task<(string Token, int ExpiresIn)> LoginAsync(LoginRequest request);
    }
}
