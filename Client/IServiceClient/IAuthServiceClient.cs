using CapManagement.Shared.DtoModels.LoginDto;

namespace CapManagement.Client.IServiceClient
{
    public interface IAuthServiceClient
    {

        Task<LoginResponse> LoginAsync(LoginRequest request);

        Task LogoutAsync();

        Task<string?> GetTokenAsync();

        Task<bool> IsLoggedInAsync();
    }
}
