using CapManagement.Shared.Models.AppicationUserModels;

namespace CapManagement.Server.IRepository
{
    public interface IUserInviteRepository
    {


        Task<UserInvite?> GetByTokenAsync(string token);
        Task<bool> HasActiveInviteAsync(string email);
        Task AddAsync(UserInvite invite);
        Task SaveChangesAsync();


    }
}
