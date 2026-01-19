using CapManagement.Server.DbContexts;
using CapManagement.Server.IRepository;
using CapManagement.Shared.Models.AppicationUserModels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CapManagement.Server.Repository
{
    public class UserInviteRepository : IUserInviteRepository
    {


        private readonly FleetDbContext _context;
        public UserInviteRepository(FleetDbContext context)
        {
            _context = context;
        }



        public async Task AddAsync(UserInvite invite)
        {
            await _context.UserInvites.AddAsync(invite);
        }

        public async Task<UserInvite?> GetByTokenAsync(string token)
        {
            return await _context.UserInvites
           .FirstOrDefaultAsync(x => x.Token == token);
        }

        public async Task<bool> HasActiveInviteAsync(string email)
        {
            return await _context.UserInvites.AnyAsync(x =>
            x.Email == email &&
            !x.IsUsed &&
            x.ExpiresAt > DateTime.UtcNow);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
