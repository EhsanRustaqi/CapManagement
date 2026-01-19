using CapManagement.Shared;

namespace CapManagement.Server.IService
{
    public interface IUserInviteService
    {


        Task <ApiResponse<bool>>CreateInviteAsync(string email, Guid companyId, string role);
        Task<ApiResponse<bool>> AcceptInviteAsync(string token, string password);


    }
}
