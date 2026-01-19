using CapManagement.Server.IService;
using CapManagement.Shared.DtoModels.LoginDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CapManagement.Server.Controllers
{

    [ApiController]
    [Route("api/[controller]")]

    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(new
                {
                    accessToken = result.Token,
                    expiresIn = result.ExpiresIn
                });
            }
            catch
            {
                return Unauthorized("Invalid credentials");
            }
        }



    }
}
