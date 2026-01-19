using CapManagement.Server.AppicationUserModels;
using CapManagement.Server.IRepository;
using CapManagement.Server.IService;
using CapManagement.Shared;
using CapManagement.Shared.Models.AppicationUserModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Security.Cryptography;

namespace CapManagement.Server.Services
{
    public class UserInviteService : IUserInviteService



    {

        private readonly IUserInviteRepository _inviteRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        public UserInviteService(IUserInviteRepository inviteRepo,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender)
        {
            _inviteRepo = inviteRepo;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Accepts an invitation using a token and creates a new user account for the invited email.
        /// The invite is validated, the user is created, assigned a role, and the invite is marked as used.
        /// </summary>
        /// <param name="token">Unique invitation token sent to the user's email</param>
        /// <param name="password">Password chosen by the user during registration</param>
        /// <returns>ApiResponse indicating success or failure of the invite acceptance</returns>
        public async Task<ApiResponse<bool>> AcceptInviteAsync(string token, string password)
        {
            try
            {
                var invite = await _inviteRepo.GetByTokenAsync(token);

                if (invite == null)
                    return new ApiResponse<bool> { Success = false, Message = "Invalid invite token." };

                if (invite.IsUsed)
                    return new ApiResponse<bool> { Success = false, Message = "Invite has already been used." };

                if (invite.ExpiresAt < DateTime.UtcNow)
                    return new ApiResponse<bool> { Success = false, Message = "Invite has expired." };

                if (await _userManager.FindByEmailAsync(invite.Email) != null)
                    return new ApiResponse<bool> { Success = false, Message = "User already exists." };

                var user = new ApplicationUser
                {
                    UserName = invite.Email,
                    Email = invite.Email,
                    CompanyId = invite.CompanyId
                };

                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "User creation failed.",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                var roleResult = await _userManager.AddToRoleAsync(user, invite.RoleName);
                if (!roleResult.Succeeded)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to assign role.",
                        Errors = roleResult.Errors.Select(e => e.Description).ToList()
                    };
                }

                invite.IsUsed = true;
               /* invite.UsedAt = DateTime.UtcNow; */// 🔥 add audit info if possible
                await _inviteRepo.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Invite accepted successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An unexpected error occurred while accepting the invite.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }



        /// <summary>
        /// Creates a new invitation for a user to join a company and sends an email with the invite link.
        /// </summary>
        /// <param name="email">Email address of the invited user</param>
        /// <param name="companyId">Company the user is invited to</param>
        /// <param name="role">Role to assign when the invite is accepted</param>
        /// <returns>ApiResponse indicating whether the invite was created and sent successfully</returns>
        public async Task<ApiResponse<bool>> CreateInviteAsync(string email, Guid companyId, string role)
        {
            // 1. User already exists
            if (await _userManager.FindByEmailAsync(email) != null)
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User already exists.",
                    Errors = { "User already exists." }
                };

            // 2. Active invite already exists
            //if (await _inviteRepo.HasActiveInviteAsync(email))
            //    return new ApiResponse<bool>
            //    {
            //        Success = false,
            //        Message = "Invite already exists.",
            //        Errors = { "An active invite already exists for this email." }
            //    };

            // 3. Create invite
            var invite = new UserInvite
            {
                Email = email,
                CompanyId = companyId,
                RoleName = role,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = DateTime.UtcNow.AddHours(48),
                IsUsed = false
            };

            await _inviteRepo.AddAsync(invite);
            await _inviteRepo.SaveChangesAsync();

            // 4. Send invite email
            var link = $"https://yourapp.com/accept-invite?token={invite.Token}";

            await _emailSender.SendEmailAsync(
                email,
                "Company Invitation",
                $"You have been invited to join a company. Click here to accept: {link}");

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Invitation sent successfully.",
                Data = true
            };
        }
    }
}
