using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapManagement.Shared.DtoModels.InviteRequestDto
{
    public class CreateInviteRequest
    {
        public string Email { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
