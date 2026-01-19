using CapManagement.Server.IService;
using CapManagement.Shared.DtoModels.InviteRequestDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CapManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInvitesController : ControllerBase
    {


        private readonly IUserInviteService _inviteService;


        public UserInvitesController(IUserInviteService inviteService)
        {
            _inviteService = inviteService;
        }


        // 1️⃣ Create invite (CompanyOwner / Owner)
        [Authorize(Roles = "Owner,CompanyOwner")]
        [HttpPost]
        public async Task<IActionResult> CreateInvite([FromBody] CreateInviteRequest request)
        {
            var result = await _inviteService.CreateInviteAsync(
                request.Email,
                request.CompanyId,
                request.Role);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // 2️⃣ Accept invite (Anonymous)
        [AllowAnonymous]
        [HttpPost("accept")]
        public async Task<IActionResult> AcceptInvite([FromBody] AcceptInviteRequest request)
        {
            var result = await _inviteService.AcceptInviteAsync(
                request.Token,
                request.Password);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


    }
}
