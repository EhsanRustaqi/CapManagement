using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapManagement.Shared.DtoModels.InviteRequestDto
{
    public class AcceptInviteRequest
    {

        public string Token { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
